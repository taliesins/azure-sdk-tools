using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal class BlockBlobUploadController : IDisposable, ITransferController
	{
		private AsyncCallCancellationHandler cancellationHandler = new AsyncCallCancellationHandler();

		private BlobTransferManager manager;

		private BlobTransferFileTransferEntry transferEntry;

		private volatile BlockBlobUploadController.State state;

		private string[] blockIdSequence;

		private ConcurrentQueue<int> unprocessedBlocks;

		private ConcurrentDictionary<string, bool> uploadedBlockIds = new ConcurrentDictionary<string, bool>();

		private CountdownEvent toUploadBlocksCountdownEvent;

		private CloudBlockBlob blob;

		private string fileName;

		private Stream inputStream;

		private long inputStreamLength;

		private bool ownsStream;

		private object inputStreamDisposeLock;

		private bool moveSource;

		private Action<object> startCallback;

		private Action<object, double, double> progressCallback;

		private Action<object, System.Exception> finishCallback;

		private object finishCallbackLock = new object();

		private TransferStatusAndTotalSpeedTracker transferStatusTracker;

		private List<System.Exception> exceptionList = new List<System.Exception>();

		private object exceptionListLock = new object();

		private MD5CryptoServiceProvider md5hash = new MD5CryptoServiceProvider();

		private bool transferFromBeginning;

		private int activeTasks;

		public bool CanAddController
		{
			get
			{
				return false;
			}
		}

		public bool CanAddMonitor
		{
			get
			{
				return false;
			}
		}

		public System.Exception Exception
		{
			get
			{
				System.Exception aggregateException;
				lock (this.exceptionListLock)
				{
					if (this.exceptionList.Count == 0)
					{
						aggregateException = null;
					}
					else if (1 != this.exceptionList.Count)
					{
						aggregateException = new AggregateException(this.exceptionList);
					}
					else
					{
						aggregateException = this.exceptionList[0];
					}
				}
				return aggregateException;
			}
		}

		public bool HasWork
		{
			get
			{
				return JustDecompileGenerated_get_HasWork();
			}
			set
			{
				JustDecompileGenerated_set_HasWork(value);
			}
		}

		private bool JustDecompileGenerated_HasWork_k__BackingField;

		public bool JustDecompileGenerated_get_HasWork()
		{
			return this.JustDecompileGenerated_HasWork_k__BackingField;
		}

		private void JustDecompileGenerated_set_HasWork(bool value)
		{
			this.JustDecompileGenerated_HasWork_k__BackingField = value;
		}

		public bool IsFinished
		{
			get
			{
				return JustDecompileGenerated_get_IsFinished();
			}
			set
			{
				JustDecompileGenerated_set_IsFinished(value);
			}
		}

		private bool JustDecompileGenerated_IsFinished_k__BackingField;

		public bool JustDecompileGenerated_get_IsFinished()
		{
			return this.JustDecompileGenerated_IsFinished_k__BackingField;
		}

		private void JustDecompileGenerated_set_IsFinished(bool value)
		{
			this.JustDecompileGenerated_IsFinished_k__BackingField = value;
		}

		public object UserData
		{
			get;
			private set;
		}

		internal BlockBlobUploadController(BlobTransferManager manager, BlobTransferFileTransferEntry transferEntry, CloudBlockBlob blob, string fileName, Stream inputStream, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, System.Exception> finishCallback, object userData)
		{
			string str;
			if (manager == null || transferEntry == null || blob == null)
			{
				if (manager == null)
				{
					str = "manager";
				}
				else
				{
					str = (transferEntry == null ? "transferEntry" : "blob");
				}
				throw new ArgumentNullException(str);
			}
			if (string.IsNullOrEmpty(fileName) && inputStream == null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothNullException = Resources.ProvideExactlyOneParameterBothNullException;
				object[] objArray = new object[] { "fileName", "inputStream" };
				throw new ArgumentException(string.Format(invariantCulture, provideExactlyOneParameterBothNullException, objArray));
			}
			if (!string.IsNullOrEmpty(fileName) && inputStream != null)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothProvidedException = Resources.ProvideExactlyOneParameterBothProvidedException;
				object[] objArray1 = new object[] { "fileName", "inputStream" };
				throw new ArgumentException(string.Format(cultureInfo, provideExactlyOneParameterBothProvidedException, objArray1));
			}
			if (moveSource)
			{
				if (string.IsNullOrEmpty(fileName))
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.CannotRemoveSourceWithoutSourceFileException, new object[0]));
				}
			}
			else if (BlobTransferEntryStatus.RemoveSource == transferEntry.Status)
			{
				CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
				string invalidInitialEntryStatusWhenMoveSourceIsOffException = Resources.InvalidInitialEntryStatusWhenMoveSourceIsOffException;
				object[] status = new object[] { transferEntry.Status };
				throw new ArgumentException(string.Format(invariantCulture1, invalidInitialEntryStatusWhenMoveSourceIsOffException, status));
			}
			this.manager = manager;
			this.transferEntry = transferEntry;
			this.transferFromBeginning = this.transferEntry.Status == BlobTransferEntryStatus.NotStarted;
			if (inputStream != null)
			{
				this.ownsStream = false;
				this.inputStream = inputStream;
			}
			else
			{
				this.ownsStream = true;
				this.fileName = fileName;
				this.inputStreamDisposeLock = new object();
			}
			this.blob = blob;
			this.moveSource = moveSource;
			this.startCallback = startCallback;
			this.progressCallback = progressCallback;
			this.finishCallback = finishCallback;
			this.UserData = userData;
			this.IsFinished = false;
			this.SetInitialStatus();
		}

		private void BeginUploadBlock(ReadDataState asyncState)
		{
			if (this.state == BlockBlobUploadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			try
			{
				this.cancellationHandler.CheckCancellation();
				this.inputStream.BeginRead(asyncState.MemoryBuffer, asyncState.BytesRead, asyncState.Length - asyncState.BytesRead, new AsyncCallback(this.UploadCallback), asyncState);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				asyncState.Dispose();
			}
		}

		private void CallbackExceptionHandler(Action callbackAction)
		{
			try
			{
				callbackAction();
			}
			catch (System.Exception exception)
			{
				throw new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, exception);
			}
		}

		public void CancelWork()
		{
			this.cancellationHandler.Cancel();
		}

		private void ChangeStatus()
		{
			if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
			{
				if (!this.moveSource)
				{
					this.transferEntry.Status = BlobTransferEntryStatus.Finished;
				}
				else
				{
					this.transferEntry.Status = BlobTransferEntryStatus.RemoveSource;
				}
			}
			else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
			{
				this.transferEntry.Status = BlobTransferEntryStatus.Finished;
			}
			if (BlobTransferEntryStatus.Finished == this.transferEntry.Status)
			{
				this.SetFinished();
				return;
			}
			this.SetHasWorkAfterStatusChanged();
		}

		private void CloseOwnedInputStream()
		{
			if (this.ownsStream && this.inputStream != null)
			{
				lock (this.inputStreamDisposeLock)
				{
					if (this.inputStream != null)
					{
						this.inputStream.Close();
						this.inputStream = null;
					}
				}
			}
		}

		private void CommitActionCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.blob.EndPutBlockList(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			this.CloseOwnedInputStream();
			this.ChangeStatus();
			asyncState.CallFinish(this);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.toUploadBlocksCountdownEvent != null)
				{
					this.toUploadBlocksCountdownEvent.Dispose();
					this.toUploadBlocksCountdownEvent = null;
				}
				this.CloseOwnedInputStream();
				if (this.md5hash != null)
				{
					this.md5hash.Clear();
					this.md5hash = null;
				}
			}
		}

		private void DownloadBlockListCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			IEnumerable<ListBlockItem> listBlockItems = null;
			try
			{
				listBlockItems = this.blob.EndDownloadBlockList(asyncResult);
			}
			catch (System.Exception exception)
			{
			}
			if (listBlockItems != null)
			{
				foreach (ListBlockItem listBlockItem in listBlockItems)
				{
					if (listBlockItem.Length != (long)this.manager.TransferOptions.BlockSize || listBlockItem.Name.Length != 20)
					{
						continue;
					}
					this.uploadedBlockIds.TryAdd(listBlockItem.Name, true);
				}
			}
			this.state = BlockBlobUploadController.State.Upload;
			this.HasWork = true;
			asyncState.CallFinish(this);
		}

		private void FetchAttributesCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.blob.EndFetchAttributes(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.HandleFetchAttributesResult(exception, asyncState);
				return;
			}
			this.HandleFetchAttributesResult(null, asyncState);
		}

		~BlockBlobUploadController()
		{
			this.Dispose(false);
		}

		private void FinishBlock(ReadDataState asyncState)
		{
			if (this.state == BlockBlobUploadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			try
			{
				this.transferStatusTracker.AddBytesTransferred((long)asyncState.Length);
			}
			catch (BlobTransferCallbackException blobTransferCallbackException)
			{
				this.SetErrorState(blobTransferCallbackException, asyncState);
				return;
			}
			if (this.toUploadBlocksCountdownEvent.Signal())
			{
				this.state = BlockBlobUploadController.State.Commit;
				this.HasWork = true;
			}
			asyncState.CallbackState.CallFinish(this);
			asyncState.Dispose();
		}

		private void FinishCallbackHandler(System.Exception ex)
		{
			try
			{
				if (this.finishCallback != null)
				{
					lock (this.finishCallbackLock)
					{
						if (this.finishCallback != null)
						{
							this.finishCallback(this.UserData, ex);
							this.finishCallback = null;
						}
					}
				}
			}
			catch (System.Exception exception)
			{
			}
		}

		private Action<Action<ITransferController>> GetCommitAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				this.md5hash.TransformFinalBlock(new byte[0], 0, 0);
				this.blob.Properties.ContentMD5=Convert.ToBase64String(this.md5hash.Hash);
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.PutBlockList);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginPutBlockList(this.blockIdSequence, null, blobRequestOptions, operationContext1, new AsyncCallback(this.CommitActionCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
				}
			};
		}

		private static string GetCurrentBlockId(string blockIdPrefix, int count)
		{
			string str = count.ToString("D6");
			byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(blockIdPrefix, str));
			return Convert.ToBase64String(bytes);
		}

		private Action<Action<ITransferController>> GetDeleteSourceAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				try
				{
					this.StartCallbackHandler();
					this.ProgressCallbackHandler(0, 100);
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				if (this.moveSource)
				{
					try
					{
						File.Delete(this.fileName);
					}
					catch (System.Exception exception1)
					{
						this.SetErrorState(exception1, new CallbackState()
						{
							FinishDelegate = finishDelegate
						});
						return;
					}
				}
				this.ChangeStatus();
				finishDelegate(this);
			};
		}

		private Action<Action<ITransferController>> GetDownloadBlockListAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.DownloadBlockList);
				AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginDownloadBlockList(BlockListingFilter.Uncommitted, accessCondition, blobRequestOptions, operationContext1, new AsyncCallback(this.DownloadBlockListCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
				}
			};
		}

		private Action<Action<ITransferController>> GetFetchAttributesAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				try
				{
					this.StartCallbackHandler();
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(this.inputStreamLength, this.manager.TransferOptions.Concurrency, new Action<double, double>(this.ProgressCallbackHandler), this.manager.GlobalUploadSpeedTracker);
				if (this.inputStreamLength <= 209715200000L)
				{
					BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.FetchAttributes);
					OperationContext operationContext = new OperationContext();
					operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
					OperationContext operationContext1 = operationContext;
					CallbackState callbackState = new CallbackState()
					{
						FinishDelegate = finishDelegate
					};
					try
					{
						this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginFetchAttributes(null, blobRequestOptions, operationContext1, new AsyncCallback(this.FetchAttributesCallback), callbackState));
					}
					catch (System.Exception exception1)
					{
						this.HandleFetchAttributesResult(exception1, callbackState);
						return;
					}
					return;
				}
				else
				{
					this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.UploadBlobSourceFileSizeTooLarge, string.Format(CultureInfo.InvariantCulture, Resources.BlobFileSizeTooLargeException, new object[] { Microsoft.WindowsAzure.Storage.DataMovement.Utils.BytesToHumanReadableSize((double)this.inputStreamLength), Resources.BlockBlob, Microsoft.WindowsAzure.Storage.DataMovement.Utils.BytesToHumanReadableSize(209715200000) })), new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
				}
			};
		}

		private Action<Action<ITransferController>> GetOpenInputStreamAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				try
				{
					this.StartCallbackHandler();
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				if (this.transferEntry.LastModified.HasValue)
				{
					try
					{
						DateTimeOffset dateTimeOffset = new DateTimeOffset(File.GetLastWriteTimeUtc(this.fileName));
						DateTimeOffset dateTimeOffset1 = dateTimeOffset;
						DateTimeOffset? lastModified = this.transferEntry.LastModified;
						if ((!lastModified.HasValue ? true : dateTimeOffset1 != lastModified.GetValueOrDefault()))
						{
							if (this.RetransferModifiedCallbackHandler(this.fileName, new CallbackState()
							{
								FinishDelegate = finishDelegate
							}))
							{
								this.transferFromBeginning = true;
								this.transferEntry.LastModified = new DateTimeOffset?(dateTimeOffset);
								this.transferEntry.CheckPoint.Clear();
							}
							else
							{
								return;
							}
						}
					}
					catch (System.Exception exception1)
					{
						this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.FailToGetSourceLastWriteTime, Resources.FailedToGetSourceLastWriteTime, exception1), new CallbackState()
						{
							FinishDelegate = finishDelegate
						});
						return;
					}
				}
				try
				{
					this.cancellationHandler.CheckCancellation();
					this.inputStream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					this.inputStreamLength = this.inputStream.Length;
				}
				catch (OperationCanceledException operationCanceledException)
				{
					this.SetErrorState(operationCanceledException, new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				catch (System.Exception exception2)
				{
					this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.OpenFileFailed, string.Format(CultureInfo.InvariantCulture, Resources.FailedToOpenFileException, new object[] { this.fileName }), exception2), new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				this.state = BlockBlobUploadController.State.FetchAttributes;
				this.HasWork = true;
				finishDelegate(this);
			};
		}

		private Action<Action<ITransferController>> GetUploadAction()
		{
			this.HasWork = false;
			if (this.blockIdSequence == null || this.unprocessedBlocks == null || this.toUploadBlocksCountdownEvent == null)
			{
				int num1 = (int)Math.Ceiling((double)this.inputStreamLength / (double)this.manager.TransferOptions.BlockSize);
				this.toUploadBlocksCountdownEvent = new CountdownEvent(num1);
				this.blockIdSequence = new string[num1];
				this.unprocessedBlocks = new ConcurrentQueue<int>();
				for (int i = 0; i < num1; i++)
				{
					this.unprocessedBlocks.Enqueue(i);
				}
			}
			return (Action<ITransferController> finishDelegate) => {
				int num;
				byte[] numArray = this.manager.MemoryManager.RequireBuffer();
				if (numArray == null)
				{
					this.HasWork = true;
					finishDelegate(this);
					return;
				}
				if (!this.unprocessedBlocks.TryDequeue(out num))
				{
					this.SetErrorState(new InvalidOperationException(), new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				long blockSize = (long)(num * this.manager.TransferOptions.BlockSize);
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				this.BeginUploadBlock(new ReadDataState()
				{
					SequenceNumber = num,
					MemoryBuffer = numArray,
					BytesRead = 0,
					StartOffset = blockSize,
					Length = (int)(Math.Min((long)(num + 1) * (long)this.manager.TransferOptions.BlockSize, this.inputStreamLength) - blockSize),
					CallbackState = callbackState,
					MemoryManager = this.manager.MemoryManager
				});
			};
		}

		public Action<Action<ITransferController>> GetWork()
		{
			this.ThrowOnErrorState();
			if (!this.HasWork)
			{
				return null;
			}
			switch (this.state)
			{
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.OpenInputStream:
				{
					return this.GetOpenInputStreamAction();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.FetchAttributes:
				{
					return this.GetFetchAttributesAction();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.DownloadBlockList:
				{
					return this.GetDownloadBlockListAction();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.Upload:
				{
					return this.GetUploadAction();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.Commit:
				{
					return this.GetCommitAction();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.DeleteSource:
				{
					return this.GetDeleteSourceAction();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.Finished:
				{
					throw new InvalidOperationException();
				}
				case (BlockBlobUploadController.State)BlockBlobUploadController.State.Error:
				{
					throw new InvalidOperationException();
				}
			}
			throw new InvalidOperationException();
		}

		private void HandleFetchAttributesResult(System.Exception e, CallbackState callbackState)
		{
			bool flag = true;
			if (e != null)
			{
				StorageException storageException = e as StorageException;
				if (storageException == null)
				{
					if (!(e is InvalidOperationException))
					{
						this.SetErrorState(e, callbackState);
						return;
					}
					this.SetErrorState(new InvalidOperationException(Resources.CannotOverwritePageBlobWithBlockBlobException, e), callbackState);
					return;
				}
				if (storageException.RequestInformation == null || storageException.RequestInformation.HttpStatusCode != 404)
				{
					this.SetErrorState(storageException, callbackState);
					return;
				}
				flag = false;
			}
			if (string.IsNullOrEmpty(this.transferEntry.BlockIdPrefix))
			{
				BlobTransferFileTransferEntry blobTransferFileTransferEntry = this.transferEntry;
				int num = (new Random()).Next();
				blobTransferFileTransferEntry.BlockIdPrefix = string.Concat(num.ToString("X8"), "-");
			}
			if ((long)0 == this.inputStreamLength)
			{
				this.blockIdSequence = new string[0];
				this.state = BlockBlobUploadController.State.Commit;
			}
			else if (flag)
			{
				if (this.blob.Properties.BlobType == null)
				{
					this.SetErrorState(new InvalidOperationException(Resources.FailedToGetBlobTypeException), callbackState);
					return;
				}
                if (this.blob.Properties.BlobType == BlobType.PageBlob)
				{
					this.SetErrorState(new InvalidOperationException(Resources.CannotOverwritePageBlobWithBlockBlobException), callbackState);
					return;
				}
				if (this.manager.TransferOptions.OverwritePromptCallback != null && !this.OverwritePromptCallbackHandler(this.blob.Uri.ToString(), callbackState))
				{
					return;
				}
				if (this.transferFromBeginning)
				{
					this.state = BlockBlobUploadController.State.Upload;
				}
				else
				{
					this.state = BlockBlobUploadController.State.DownloadBlockList;
				}
			}
			else if (this.transferFromBeginning)
			{
				this.state = BlockBlobUploadController.State.Upload;
			}
			else
			{
				this.state = BlockBlobUploadController.State.DownloadBlockList;
			}
			this.HasWork = true;
			callbackState.CallFinish(this);
		}

		private bool OverwritePromptCallbackHandler(string desFileName, CallbackState callbackState)
		{
			return this.PromptCallBackExceptionHandler(() => {
				if (this.manager.TransferOptions.OverwritePromptCallback != null && !this.manager.TransferOptions.OverwritePromptCallback(desFileName))
				{
					throw new OperationCanceledException(Resources.OverwriteCallbackCancelTransferException);
				}
				return true;
			}, callbackState);
		}

		public int PostWork()
		{
			return Interlocked.Decrement(ref this.activeTasks);
		}

		public void PreWork()
		{
			Interlocked.Increment(ref this.activeTasks);
		}

		private void ProgressCallbackHandler(double speed, double progress)
		{
			if (this.progressCallback != null)
			{
				this.CallbackExceptionHandler(() => this.progressCallback(this.UserData, speed, progress));
			}
		}

		private bool PromptCallBackExceptionHandler(BlockBlobUploadController.PromptCallbackAction promptcallbackAction, CallbackState callbackState)
		{
			bool flag;
			try
			{
				flag = promptcallbackAction();
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, callbackState);
				flag = false;
			}
			return flag;
		}

		private void PutBlockCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;
			try
			{
				this.blob.EndPutBlock(asyncResult);
				this.FinishBlock(asyncState);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				asyncState.Dispose();
			}
		}

		private bool RetransferModifiedCallbackHandler(string sourceFileName, CallbackState callbackState)
		{
			return this.PromptCallBackExceptionHandler(() => {
				if (this.manager.TransferOptions.RetransferModifiedCallback == null || !this.manager.TransferOptions.RetransferModifiedCallback(sourceFileName))
				{
					throw new InvalidOperationException(Resources.SourceFileHasBeenChangedException);
				}
				return true;
			}, callbackState);
		}

		private void SetErrorState(System.Exception ex, ReadDataState readState)
		{
			this.SetErrorState(ex, readState.CallbackState);
			readState.Dispose();
		}

		private void SetErrorState(System.Exception ex, CallbackState callbackState)
		{
			lock (this.exceptionListLock)
			{
				this.exceptionList.Add(ex);
			}
			this.state = BlockBlobUploadController.State.Error;
			this.HasWork = false;
			this.IsFinished = true;
			if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
			{
				this.CloseOwnedInputStream();
			}
			this.FinishCallbackHandler(ex);
			callbackState.CallFinish(this);
		}

		private void SetFinished()
		{
			this.state = BlockBlobUploadController.State.Finished;
			this.HasWork = false;
			this.IsFinished = true;
			this.FinishCallbackHandler(null);
		}

		private void SetHasWorkAfterStatusChanged()
		{
			if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
			{
				if (this.inputStream != null)
				{
					if (!this.inputStream.CanRead)
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						string streamMustSupportReadException = Resources.StreamMustSupportReadException;
						object[] objArray = new object[] { "inputStream" };
						throw new NotSupportedException(string.Format(invariantCulture, streamMustSupportReadException, objArray));
					}
					if (!this.inputStream.CanSeek)
					{
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						string streamMustSupportSeekException = Resources.StreamMustSupportSeekException;
						object[] objArray1 = new object[] { "inputStream" };
						throw new NotSupportedException(string.Format(cultureInfo, streamMustSupportSeekException, objArray1));
					}
					this.inputStreamLength = this.inputStream.Length;
					if ((long)0 != this.inputStream.Position)
					{
						this.inputStream.Seek((long)0, SeekOrigin.Begin);
					}
					this.state = BlockBlobUploadController.State.FetchAttributes;
				}
				else
				{
					this.state = BlockBlobUploadController.State.OpenInputStream;
				}
			}
			else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
			{
				this.state = BlockBlobUploadController.State.DeleteSource;
			}
			this.HasWork = true;
		}

		private void SetInitialStatus()
		{
			object[] status;
			CultureInfo invariantCulture;
			string invalidInitialEntryStatusForControllerException;
			switch (this.transferEntry.Status)
			{
				case BlobTransferEntryStatus.NotStarted:
				{
					this.transferEntry.Status = BlobTransferEntryStatus.Transfer;
					this.SetHasWorkAfterStatusChanged();
					return;
				}
				case BlobTransferEntryStatus.Transfer:
				case BlobTransferEntryStatus.RemoveSource:
				{
					this.SetHasWorkAfterStatusChanged();
					return;
				}
				case BlobTransferEntryStatus.Monitor:
				case BlobTransferEntryStatus.Finished:
				{
					invariantCulture = CultureInfo.InvariantCulture;
					invalidInitialEntryStatusForControllerException = Resources.InvalidInitialEntryStatusForControllerException;
					status = new object[] { this.transferEntry.Status, this.GetType().Name };
					throw new ArgumentException(string.Format(invariantCulture, invalidInitialEntryStatusForControllerException, status));
				}
				default:
				{
					invariantCulture = CultureInfo.InvariantCulture;
					invalidInitialEntryStatusForControllerException = Resources.InvalidInitialEntryStatusForControllerException;
					status = new object[] { this.transferEntry.Status, this.GetType().Name };
					throw new ArgumentException(string.Format(invariantCulture, invalidInitialEntryStatusForControllerException, status));
				}
			}
		}

		private void StartCallbackHandler()
		{
			if (this.startCallback != null)
			{
				this.CallbackExceptionHandler(() => {
					this.startCallback(this.UserData);
					this.startCallback = null;
				});
			}
		}

		private void ThrowOnErrorState()
		{
			if (this.state == BlockBlobUploadController.State.Error)
			{
				throw this.Exception;
			}
		}

		private void UploadCallback(IAsyncResult asyncResult)
		{
			int num;
			ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;
			try
			{
				num = this.inputStream.EndRead(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				asyncState.Dispose();
				return;
			}
			if (this.state != BlockBlobUploadController.State.Error)
			{
				ReadDataState bytesRead = asyncState;
				bytesRead.BytesRead = bytesRead.BytesRead + num;
				if (asyncState.BytesRead < asyncState.Length)
				{
					this.BeginUploadBlock(asyncState);
					return;
				}
				try
				{
					this.cancellationHandler.CheckCancellation();
					this.md5hash.TransformBlock(asyncState.MemoryBuffer, 0, asyncState.Length, null, 0);
				}
				catch (System.Exception exception1)
				{
					this.SetErrorState(exception1, asyncState.CallbackState);
					asyncState.Dispose();
					return;
				}
				this.HasWork = !this.unprocessedBlocks.IsEmpty;
				string currentBlockId = BlockBlobUploadController.GetCurrentBlockId(this.transferEntry.BlockIdPrefix, asyncState.SequenceNumber);
				this.blockIdSequence[asyncState.SequenceNumber] = currentBlockId;
				if (!this.uploadedBlockIds.TryAdd(currentBlockId, false))
				{
					this.FinishBlock(asyncState);
				}
				else
				{
					try
					{
						BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.PutBlock);
						OperationContext operationContext = new OperationContext();
						operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
						OperationContext operationContext1 = operationContext;
						asyncState.MemoryStream = new MemoryStream(asyncState.MemoryBuffer, 0, asyncState.Length);
						this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginPutBlock(currentBlockId, asyncState.MemoryStream, null, null, blobRequestOptions, operationContext1, new AsyncCallback(this.PutBlockCallback), asyncState));
					}
					catch (System.Exception exception2)
					{
						this.SetErrorState(exception2, asyncState.CallbackState);
						asyncState.Dispose();
						return;
					}
				}
				return;
			}
			else
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
			}
		}

		private delegate bool PromptCallbackAction();

		private enum State
		{
			OpenInputStream,
			FetchAttributes,
			DownloadBlockList,
			Upload,
			Commit,
			DeleteSource,
			Finished,
			Error
		}
	}
}