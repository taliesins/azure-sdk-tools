using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal class PageBlobUploadController : IDisposable, ITransferController
	{
		private const long PageBlobPageSize = 512L;

		private AsyncCallCancellationHandler cancellationHandler = new AsyncCallCancellationHandler();

		private BlobTransferManager manager;

		private BlobTransferFileTransferEntry transferEntry;

		private Queue<long> lastUploadWindow;

		private volatile PageBlobUploadController.State state;

		private CountdownEvent toUploadChunksCountdownEvent;

		private CloudPageBlob blob;

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

		private bool needClearEmptyBlocks;

		private MD5HashStream md5HashStream;

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

		internal PageBlobUploadController(BlobTransferManager manager, BlobTransferFileTransferEntry transferEntry, CloudPageBlob blob, string fileName, Stream inputStream, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, System.Exception> finishCallback, object userData)
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
			if (inputStream != null)
			{
				this.ownsStream = false;
				this.inputStream = inputStream;
				this.md5HashStream = new MD5HashStream(this.inputStream, this.transferEntry.CheckPoint.EntryTransferOffset, true);
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

		private void BeginUploadChunk(ReadDataState asyncState)
		{
			if (this.state == PageBlobUploadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			try
			{
				this.cancellationHandler.CheckCancellation();
				this.md5HashStream.BeginRead(asyncState.StartOffset + (long)asyncState.BytesRead, asyncState.MemoryBuffer, asyncState.BytesRead, asyncState.Length - asyncState.BytesRead, new AsyncCallback(this.UploadCallback), asyncState);
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

		private void ClearPagesCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;
			try
			{
				this.blob.EndClearPages(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				asyncState.Dispose();
				return;
			}
			if (this.state != PageBlobUploadController.State.Error)
			{
				this.FinishChunk(asyncState);
				return;
			}
			asyncState.CallbackState.CallFinish(this);
			asyncState.Dispose();
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

		private void CommitCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.blob.EndSetProperties(asyncResult);
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

		private void CreateCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.blob.EndCreate(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			if ((long)0 != this.inputStreamLength)
			{
				this.InitUpload(asyncState);
				this.needClearEmptyBlocks = false;
				if (!this.md5HashStream.FinishedSeparateMd5Calculator)
				{
					this.state = PageBlobUploadController.State.CalculateMD5;
				}
				else
				{
					this.state = PageBlobUploadController.State.Upload;
				}
				this.HasWork = true;
			}
			else
			{
				this.SetCommit();
			}
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
				if (this.toUploadChunksCountdownEvent != null)
				{
					this.toUploadChunksCountdownEvent.Dispose();
					this.toUploadChunksCountdownEvent = null;
				}
				if (this.md5HashStream != null)
				{
					this.md5HashStream.Dispose();
					this.md5HashStream = null;
				}
				this.CloseOwnedInputStream();
			}
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

		~PageBlobUploadController()
		{
			this.Dispose(false);
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

		private void FinishChunk(ReadDataState asyncState)
		{
			if (this.state == PageBlobUploadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			lock (this.transferEntry.EntryLock)
			{
				this.transferEntry.CheckPoint.UploadWindow.Remove(asyncState.StartOffset);
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
			if (this.toUploadChunksCountdownEvent.Signal())
			{
				if (!this.md5HashStream.SucceededSeparateMd5Calculator)
				{
					asyncState.CallbackState.CallFinish(this);
					asyncState.Dispose();
					return;
				}
				this.SetCommit();
			}
			asyncState.CallbackState.CallFinish(this);
			asyncState.Dispose();
		}

		private Action<Action<ITransferController>> GetCalculateMD5Action()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				this.HasWork = true;
				this.state = PageBlobUploadController.State.Upload;
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				ThreadPool.QueueUserWorkItem((object param0) => {
					try
					{
						this.md5HashStream.CalculateMd5(this.manager.MemoryManager);
					}
					catch (System.Exception exception)
					{
						this.SetErrorState(exception, callbackState);
						return;
					}
					finishDelegate(this);
				});
			};
		}

		private Action<Action<ITransferController>> GetCommitAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.SetMetadata);
					OperationContext operationContext = new OperationContext();
					operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
					OperationContext operationContext1 = operationContext;
					this.md5HashStream.MD5HashTransformFinalBlock(new byte[0], 0, 0);
					this.blob.Properties.ContentMD5=Convert.ToBase64String(this.md5HashStream.Hash);
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginSetProperties(null, blobRequestOptions, operationContext1, new AsyncCallback(this.CommitCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
				}
			};
		}

		private Action<Action<ITransferController>> GetCreateAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.CreatePageBlob);
					OperationContext operationContext = new OperationContext();
					operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
					OperationContext operationContext1 = operationContext;
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginCreate(this.inputStreamLength, null, blobRequestOptions, operationContext1, new AsyncCallback(this.CreateCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
				}
			};
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
				if (this.inputStreamLength <= 1099511627776L)
				{
					if ((long)0 != this.inputStreamLength % (long)512)
					{
						this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.UploadBlobSourceFileSizeInvalid, string.Format(CultureInfo.InvariantCulture, Resources.BlobFileSizeInvalidException, new object[] { Utils.BytesToHumanReadableSize((double)this.inputStreamLength), Resources.PageBlob, Utils.BytesToHumanReadableSize(512) })), new CallbackState()
						{
							FinishDelegate = finishDelegate
						});
						return;
					}
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
					this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.UploadBlobSourceFileSizeTooLarge, string.Format(CultureInfo.InvariantCulture, Resources.BlobFileSizeTooLargeException, new object[] { Utils.BytesToHumanReadableSize((double)this.inputStreamLength), Resources.PageBlob, Utils.BytesToHumanReadableSize(1099511627776) })), new CallbackState()
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
				if (this.transferEntry.CheckPoint.UploadWindow == null)
				{
					this.transferEntry.CheckPoint.UploadWindow = new List<long>();
				}
				else if (this.transferEntry.CheckPoint.UploadWindow.Any<long>())
				{
					this.lastUploadWindow = new Queue<long>(this.transferEntry.CheckPoint.UploadWindow);
				}
				try
				{
					this.cancellationHandler.CheckCancellation();
					this.inputStream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					this.inputStreamLength = this.inputStream.Length;
					this.md5HashStream = new MD5HashStream(this.inputStream, this.transferEntry.CheckPoint.EntryTransferOffset, true);
				}
				catch (OperationCanceledException operationCanceledException)
				{
					this.SetErrorState(operationCanceledException, new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
				}
				catch (System.Exception exception2)
				{
					this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.OpenFileFailed, string.Format(CultureInfo.InvariantCulture, Resources.FailedToOpenFileException, new object[] { this.fileName }), exception2), new CallbackState()
					{
						FinishDelegate = finishDelegate
					});
					return;
				}
				this.state = PageBlobUploadController.State.FetchAttributes;
				this.HasWork = true;
				finishDelegate(this);
			};
		}

		private Action<Action<ITransferController>> GetResizeAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.CreatePageBlob);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginResize(this.inputStreamLength, null, blobRequestOptions, operationContext1, new AsyncCallback(this.ResizeCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
				}
			};
		}

		private Action<Action<ITransferController>> GetUploadAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				byte[] numArray = this.manager.MemoryManager.RequireBuffer();
				if (numArray == null)
				{
					this.HasWork = true;
					finishDelegate(this);
					return;
				}
				long entryTransferOffset = (long)0;
				if (this.lastUploadWindow == null || !this.lastUploadWindow.Any<long>())
				{
					bool flag = false;
					lock (this.transferEntry.EntryLock)
					{
						if (this.transferEntry.CheckPoint.UploadWindow.Count < 128)
						{
							entryTransferOffset = this.transferEntry.CheckPoint.EntryTransferOffset;
							flag = true;
							if (this.transferEntry.CheckPoint.EntryTransferOffset <= this.inputStreamLength)
							{
								this.transferEntry.CheckPoint.UploadWindow.Add(entryTransferOffset);
								this.transferEntry.CheckPoint.EntryTransferOffset = Math.Min(this.transferEntry.CheckPoint.EntryTransferOffset + (long)this.manager.TransferOptions.BlockSize, this.inputStreamLength);
							}
						}
					}
					if (!flag)
					{
						this.HasWork = true;
						finishDelegate(this);
						this.manager.MemoryManager.ReleaseBuffer(numArray);
						return;
					}
				}
				else
				{
					entryTransferOffset = this.lastUploadWindow.Dequeue();
				}
				if (entryTransferOffset > this.inputStreamLength)
				{
					this.SetErrorState(new InvalidOperationException(Resources.SourceFileHasBeenChangedException), callbackState);
					this.manager.MemoryManager.ReleaseBuffer(numArray);
					return;
				}
				this.transferStatusTracker.AddBytesTransferred((long)0);
				ReadDataState readDataState = new ReadDataState()
				{
					MemoryBuffer = numArray,
					BytesRead = 0,
					StartOffset = entryTransferOffset,
					Length = (int)Math.Min((long)this.manager.TransferOptions.BlockSize, this.inputStreamLength - entryTransferOffset),
					CallbackState = callbackState,
					MemoryManager = this.manager.MemoryManager
				};
				if (entryTransferOffset == this.inputStreamLength)
				{
					this.FinishChunk(readDataState);
					return;
				}
				this.BeginUploadChunk(readDataState);
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
				case (PageBlobUploadController.State)PageBlobUploadController.State.OpenInputStream:
				{
					return this.GetOpenInputStreamAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.FetchAttributes:
				{
					return this.GetFetchAttributesAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.Create:
				{
					return this.GetCreateAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.Resize:
				{
					return this.GetResizeAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.CalculateMD5:
				{
					return this.GetCalculateMD5Action();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.Upload:
				{
					return this.GetUploadAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.Commit:
				{
					return this.GetCommitAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.DeleteSource:
				{
					return this.GetDeleteSourceAction();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.Finished:
				{
					throw new InvalidOperationException();
				}
				case (PageBlobUploadController.State)PageBlobUploadController.State.Error:
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
					this.SetErrorState(new InvalidOperationException(Resources.CannotOverwriteBlockBlobWithPageBlobException, e), callbackState);
					return;
				}
				if (storageException.RequestInformation == null || storageException.RequestInformation.HttpStatusCode != 404)
				{
					this.SetErrorState(storageException, callbackState);
					return;
				}
				flag = false;
			}
			if (!flag)
			{
				this.state = PageBlobUploadController.State.Create;
			}
			else
			{
				if (this.blob.Properties.BlobType == null)
				{
					this.SetErrorState(new InvalidOperationException(Resources.FailedToGetBlobTypeException), callbackState);
					return;
				}
				if (this.blob.Properties.BlobType == BlobType.BlockBlob)
				{
					this.SetErrorState(new InvalidOperationException(Resources.CannotOverwriteBlockBlobWithPageBlobException), callbackState);
					return;
				}
				if (this.manager.TransferOptions.OverwritePromptCallback != null && !this.OverwritePromptCallbackHandler(this.blob.Uri.ToString(), callbackState))
				{
					return;
				}
				this.state = PageBlobUploadController.State.Resize;
			}
			this.HasWork = true;
			callbackState.CallFinish(this);
		}

		private void InitUpload(CallbackState callbackState)
		{
			if (this.toUploadChunksCountdownEvent == null)
			{
				if (this.transferEntry.CheckPoint.EntryTransferOffset != this.inputStreamLength && (long)0 != this.transferEntry.CheckPoint.EntryTransferOffset % (long)this.manager.TransferOptions.BlockSize)
				{
					this.SetErrorState(new FormatException(Resources.RestartableInfoCorruptedException), callbackState);
					return;
				}
				int num = (int)Math.Ceiling((double)(this.inputStreamLength - this.transferEntry.CheckPoint.EntryTransferOffset) / (double)this.manager.TransferOptions.BlockSize) + this.transferEntry.CheckPoint.UploadWindow.Count + 1;
				this.toUploadChunksCountdownEvent = new CountdownEvent(num);
				long entryTransferOffset = this.transferEntry.CheckPoint.EntryTransferOffset - (long)(this.transferEntry.CheckPoint.UploadWindow.Count * this.manager.TransferOptions.BlockSize);
				this.transferStatusTracker.AddBytesTransferred(entryTransferOffset);
			}
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

		private bool PromptCallBackExceptionHandler(PageBlobUploadController.PromptCallbackAction promptcallbackAction, CallbackState callbackState)
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

		private void ResizeCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.blob.EndResize(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			if ((long)0 != this.inputStreamLength)
			{
				this.InitUpload(asyncState);
				this.needClearEmptyBlocks = true;
				if (!this.md5HashStream.FinishedSeparateMd5Calculator)
				{
					this.state = PageBlobUploadController.State.CalculateMD5;
				}
				else
				{
					this.state = PageBlobUploadController.State.Upload;
				}
				this.HasWork = true;
			}
			else
			{
				this.SetCommit();
			}
			asyncState.CallFinish(this);
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

		private void SetCommit()
		{
			this.state = PageBlobUploadController.State.Commit;
			this.HasWork = true;
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
			this.state = PageBlobUploadController.State.Error;
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
			this.state = PageBlobUploadController.State.Finished;
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
					this.state = PageBlobUploadController.State.FetchAttributes;
				}
				else
				{
					this.state = PageBlobUploadController.State.OpenInputStream;
				}
			}
			else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
			{
				this.state = PageBlobUploadController.State.DeleteSource;
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
			if (this.state == PageBlobUploadController.State.Error)
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
				num = this.md5HashStream.EndRead(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				asyncState.Dispose();
				return;
			}
			if (this.state != PageBlobUploadController.State.Error)
			{
				ReadDataState bytesRead = asyncState;
				bytesRead.BytesRead = bytesRead.BytesRead + num;
				if (asyncState.BytesRead < asyncState.Length)
				{
					this.BeginUploadChunk(asyncState);
					return;
				}
				bool flag = true;
				try
				{
					this.cancellationHandler.CheckCancellation();
					if (!this.md5HashStream.MD5HashTransformBlock(asyncState.StartOffset, asyncState.MemoryBuffer, 0, asyncState.Length, null, 0))
					{
						asyncState.CallbackState.CallFinish(this);
						asyncState.Dispose();
						return;
					}
				}
				catch (System.Exception exception1)
				{
					this.SetErrorState(exception1, asyncState.CallbackState);
					asyncState.Dispose();
					return;
				}
				lock (this.transferEntry.EntryLock)
				{
					this.HasWork = (this.transferEntry.CheckPoint.EntryTransferOffset >= this.inputStreamLength || this.transferEntry.CheckPoint.UploadWindow.Count >= 128 ? this.transferEntry.CheckPoint.UploadWindow.Count > 0 : true);
				}
				int num1 = 0;
				while (num1 < asyncState.Length)
				{
					if (asyncState.MemoryBuffer[num1] == 0)
					{
						num1++;
					}
					else
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					try
					{
						BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.WritePages);
						OperationContext operationContext = new OperationContext();
						operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
						OperationContext operationContext1 = operationContext;
						asyncState.MemoryStream = new MemoryStream(asyncState.MemoryBuffer, 0, asyncState.Length);
						this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginWritePages(asyncState.MemoryStream, asyncState.StartOffset, null, null, blobRequestOptions, operationContext1, new AsyncCallback(this.WritePagesCallback), asyncState));
					}
					catch (System.Exception exception2)
					{
						this.SetErrorState(exception2, asyncState.CallbackState);
						asyncState.Dispose();
						return;
					}
				}
				else if (!this.needClearEmptyBlocks)
				{
					this.FinishChunk(asyncState);
				}
				else
				{
					try
					{
						BlobRequestOptions blobRequestOption = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.ClearPages);
						OperationContext operationContext2 = new OperationContext();
						operationContext2.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
						OperationContext operationContext3 = operationContext2;
						this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginClearPages(asyncState.StartOffset, (long)asyncState.Length, null, blobRequestOption, operationContext3, new AsyncCallback(this.ClearPagesCallback), asyncState));
					}
					catch (System.Exception exception3)
					{
						this.SetErrorState(exception3, asyncState.CallbackState);
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

		private void WritePagesCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;
			try
			{
				this.blob.EndWritePages(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				asyncState.Dispose();
				return;
			}
			if (this.state != PageBlobUploadController.State.Error)
			{
				this.FinishChunk(asyncState);
				return;
			}
			asyncState.CallbackState.CallFinish(this);
			asyncState.Dispose();
		}

		private delegate bool PromptCallbackAction();

		private enum State
		{
			OpenInputStream,
			FetchAttributes,
			Create,
			Resize,
			CalculateMD5,
			Upload,
			Commit,
			DeleteSource,
			Finished,
			Error
		}
	}
}