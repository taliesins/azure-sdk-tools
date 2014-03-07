using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using Microsoft.WindowsAzure.Storage.DataMovement.Extensions;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal class BlobDownloadController : IDisposable, ITransferController
	{
		private AsyncCallCancellationHandler cancellationHandler = new AsyncCallCancellationHandler();

		private BlobTransferManager manager;

		private BlobTransferFileTransferEntry transferEntry;

		private volatile BlobDownloadController.State state;

		private ICloudBlob blob;

		private CloudBlockBlob blockBlob;

		private CloudPageBlob pageBlob;

		private string fileName;

		private Stream outputStream;

		private bool ownsStream;

		private object outputStreamDisposeLock;

		private bool moveSource;

		private CountdownEvent toDownloadItemsCountdownEvent;

		private TransferStatusAndTotalSpeedTracker transferStatusTracker;

		private Action<object> startCallback;

		private Action<object, double, double> progressCallback;

		private Action<object, System.Exception> finishCallback;

		private object finishCallbackLock = new object();

		private List<BlobDownloadController.BlockData> blocksToDownload;

		private volatile bool hasActiveFileWriter;

		private ConcurrentDictionary<string, BlobDownloadController.BlockCacheEntry> availableBlockData;

		private ConcurrentDictionary<long, byte[]> availablePageRangeData;

		private ConcurrentDictionary<string, object> currentBlocksDownloading;

		private int nextDownloadIndex;

		private volatile int nextWriteIndex;

		private List<BlobDownloadController.PageBlobRange> pageRangeList;

		private CountdownEvent getPageRangesCountDownEvent;

		private HashSet<BlobDownloadController.PageRangesSpan> pageRangesSpanSet;

		private object getPageRangesLock;

		private int getPageRangesSpanIndex;

		private int getPageRangesSpanCount;

		private List<System.Exception> exceptionList = new List<System.Exception>();

		private object exceptionListLock = new object();

		private MD5HashStream md5HashStream;

		private bool checkMd5;

		private long reservedMemory;

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

		internal BlobDownloadController(BlobTransferManager manager, BlobTransferFileTransferEntry transferEntry, ICloudBlob blob, string fileName, Stream outputStream, bool checkMd5, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, System.Exception> finishCallback, object userData)
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
			if (string.IsNullOrEmpty(fileName) && outputStream == null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothNullException = Resources.ProvideExactlyOneParameterBothNullException;
				object[] objArray = new object[] { "fileName", "outputStream" };
				throw new ArgumentException(string.Format(invariantCulture, provideExactlyOneParameterBothNullException, objArray));
			}
			if (!string.IsNullOrEmpty(fileName) && outputStream != null)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothProvidedException = Resources.ProvideExactlyOneParameterBothProvidedException;
				object[] objArray1 = new object[] { "fileName", "outputStream" };
				throw new ArgumentException(string.Format(cultureInfo, provideExactlyOneParameterBothProvidedException, objArray1));
			}
			if (!moveSource && BlobTransferEntryStatus.RemoveSource == transferEntry.Status)
			{
				CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
				string invalidInitialEntryStatusWhenMoveSourceIsOffException = Resources.InvalidInitialEntryStatusWhenMoveSourceIsOffException;
				object[] status = new object[] { transferEntry.Status };
				throw new ArgumentException(string.Format(invariantCulture1, invalidInitialEntryStatusWhenMoveSourceIsOffException, status));
			}
			this.manager = manager;
			this.transferEntry = transferEntry;
			if (outputStream != null)
			{
				this.ownsStream = false;
				this.outputStream = outputStream;
			}
			else
			{
				this.ownsStream = true;
				this.fileName = fileName;
				this.outputStreamDisposeLock = new object();
			}
			this.blob = blob;
			this.moveSource = moveSource;
			this.startCallback = startCallback;
			this.progressCallback = progressCallback;
			this.finishCallback = finishCallback;
			this.UserData = userData;
			this.IsFinished = false;
			this.checkMd5 = checkMd5;
			this.SetInitialStatus();
		}

		private void ArrangePageRanges()
		{
			long endOffset = (long)-1;
			IEnumerator<BlobDownloadController.PageRangesSpan> enumerator = (
				from pageRanges in this.pageRangesSpanSet
				orderby pageRanges.StartOffset
				select pageRanges).GetEnumerator();
			bool flag = enumerator.MoveNext();
			if (flag)
			{
				BlobDownloadController.PageRangesSpan current = enumerator.Current;
				while (flag)
				{
					flag = enumerator.MoveNext();
					if (current.PageRanges.Any<PageRange>())
					{
						if (flag)
						{
							BlobDownloadController.PageRangesSpan pageRangesSpan = enumerator.Current;
							if (pageRangesSpan.PageRanges.Any<PageRange>() && current.PageRanges.Last<PageRange>().EndOffset + (long)1 == pageRangesSpan.PageRanges.First<PageRange>().StartOffset)
							{
								PageRange pageRange = new PageRange(current.PageRanges.Last<PageRange>().StartOffset, pageRangesSpan.PageRanges.First<PageRange>().EndOffset);
								current.PageRanges.RemoveAt(current.PageRanges.Count - 1);
								pageRangesSpan.PageRanges.RemoveAt(0);
								current.PageRanges.Add(pageRange);
								current.EndOffset = pageRange.EndOffset;
								pageRangesSpan.StartOffset = pageRange.EndOffset + (long)1;
								if (pageRangesSpan.EndOffset == pageRange.EndOffset)
								{
									continue;
								}
							}
						}
						foreach (PageRange pageRange1 in current.PageRanges)
						{
							if (endOffset != pageRange1.StartOffset - (long)1)
							{
								List<BlobDownloadController.PageBlobRange> pageBlobRanges = this.pageRangeList;
								BlobDownloadController.PageBlobRange pageBlobRange = new BlobDownloadController.PageBlobRange()
								{
									StartOffset = endOffset + (long)1,
									EndOffset = pageRange1.StartOffset - (long)1,
									HasData = false
								};
								pageBlobRanges.AddRange(pageBlobRange.SplitRanges((long)this.manager.TransferOptions.BlockSize));
							}
							List<BlobDownloadController.PageBlobRange> pageBlobRanges1 = this.pageRangeList;
							BlobDownloadController.PageBlobRange pageBlobRange1 = new BlobDownloadController.PageBlobRange()
							{
								StartOffset = pageRange1.StartOffset,
								EndOffset = pageRange1.EndOffset,
								HasData = true
							};
							pageBlobRanges1.AddRange(pageBlobRange1.SplitRanges((long)this.manager.TransferOptions.BlockSize));
							endOffset = pageRange1.EndOffset;
						}
						current = enumerator.Current;
					}
					else
					{
						current = enumerator.Current;
					}
				}
			}
			if (endOffset < this.blob.Properties.Length - (long)1)
			{
				List<BlobDownloadController.PageBlobRange> pageBlobRanges2 = this.pageRangeList;
				BlobDownloadController.PageBlobRange pageBlobRange2 = new BlobDownloadController.PageBlobRange()
				{
					StartOffset = endOffset + (long)1,
					EndOffset = this.blob.Properties.Length - (long)1,
					HasData = false
				};
				pageBlobRanges2.AddRange(pageBlobRange2.SplitRanges((long)this.manager.TransferOptions.BlockSize));
			}
		}

		private void BeginDownloadBlock(BlobDownloadController.DownloadBlockState asyncState)
		{
			if (this.state == BlobDownloadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			try
			{
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.OpenRead);
				AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				asyncState.OperationContext = operationContext1;
				asyncState.MemoryStream = new MemoryStream(asyncState.MemoryBuffer, 0, asyncState.Length);
				this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginDownloadRangeToStream(asyncState.MemoryStream, new long?(asyncState.StartOffset), new long?((long)asyncState.Length), accessCondition, blobRequestOptions, operationContext1, new AsyncCallback(this.DownloadBlockBlobCallback), asyncState));
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
			}
		}

		private void BeginDownloadPageRange(BlobDownloadController.DownloadPageState asyncState)
		{
			if (this.state == BlobDownloadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			if (!asyncState.PageRange.HasData)
			{
				Array.Clear(asyncState.MemoryBuffer, 0, (int)asyncState.MemoryBuffer.Length);
				this.availablePageRangeData.TryAdd(asyncState.StartOffset, asyncState.MemoryBuffer);
				asyncState.MemoryBuffer = null;
				this.SetPageRangeDownloadHasWork();
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
			}
			else
			{
				try
				{
					BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.OpenRead);
					AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag);
					OperationContext operationContext = new OperationContext();
					operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
					OperationContext operationContext1 = operationContext;
					asyncState.OperationContext = operationContext1;
					asyncState.MemoryStream = new MemoryStream(asyncState.MemoryBuffer, 0, asyncState.Length);
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginDownloadRangeToStream(asyncState.MemoryStream, new long?(asyncState.StartOffset), new long?((long)asyncState.Length), accessCondition, blobRequestOptions, operationContext1, new AsyncCallback(this.DownloadPageRangeCallback), asyncState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, asyncState);
				}
			}
		}

		private void BeginWriteBlockData(BlobDownloadController.DownloadBlockState asyncState)
		{
			if (this.state == BlobDownloadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			try
			{
				this.cancellationHandler.CheckCancellation();
				this.md5HashStream.BeginWrite(asyncState.StartOffset, asyncState.MemoryBuffer, 0, asyncState.Length, new AsyncCallback(this.WriteBlockDataCallback), asyncState);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
			}
		}

		private void BeginWritePageRangeData(BlobDownloadController.DownloadPageState asyncState)
		{
			if (this.state == BlobDownloadController.State.Error)
			{
				asyncState.CallbackState.CallFinish(this);
				asyncState.Dispose();
				return;
			}
			try
			{
				this.cancellationHandler.CheckCancellation();
				this.md5HashStream.BeginWrite(asyncState.StartOffset, asyncState.MemoryBuffer, 0, asyncState.Length, new AsyncCallback(this.WritePageRangeDataCallback), asyncState);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
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

		private void ClearForGetPageRanges()
		{
			this.pageRangesSpanSet = null;
			this.getPageRangesLock = null;
			this.getPageRangesCountDownEvent = null;
		}

		private void CloseOwnedOutputStream()
		{
			if (this.ownsStream && this.outputStream != null)
			{
				lock (this.outputStreamDisposeLock)
				{
					if (this.outputStream != null)
					{
						this.outputStream.Close();
						this.outputStream = null;
					}
				}
			}
		}

		private void DeleteSourceCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.blob.EndDelete(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
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
				if (this.toDownloadItemsCountdownEvent != null)
				{
					this.toDownloadItemsCountdownEvent.Dispose();
					this.toDownloadItemsCountdownEvent = null;
				}
				if (this.md5HashStream != null)
				{
					this.md5HashStream.Dispose();
					this.md5HashStream = null;
				}
				this.CloseOwnedOutputStream();
			}
		}

		private void DownloadBlockBlobCallback(IAsyncResult asyncResult)
		{
			object obj;
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			BlobDownloadController.DownloadBlockState asyncState = asyncResult.AsyncState as BlobDownloadController.DownloadBlockState;
			try
			{
				this.blob.EndDownloadRangeToStream(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			asyncState.BlockCacheEntry.MemoryBuffer = asyncState.MemoryBuffer;
			asyncState.MemoryBuffer = null;
			this.currentBlocksDownloading.TryRemove(asyncState.BlockData.BlockName, out obj);
			this.SetBlockDownloadHasWork();
			asyncState.CallbackState.CallFinish(this);
			asyncState.Dispose();
		}

		private void DownloadBlockListCallback(IAsyncResult asyncResult)
		{
			int num = 0;
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			IEnumerable<ListBlockItem> listBlockItems = null;
			try
			{
				listBlockItems = this.blockBlob.EndDownloadBlockList(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			this.blocksToDownload = new List<BlobDownloadController.BlockData>();
			this.availableBlockData = new ConcurrentDictionary<string, BlobDownloadController.BlockCacheEntry>();
			this.currentBlocksDownloading = new ConcurrentDictionary<string, object>();
			this.nextDownloadIndex = 0;
			
			long num1 = (long)0;
			long entryTransferOffset = this.transferEntry.CheckPoint.EntryTransferOffset;
			if (!listBlockItems.Any<ListBlockItem>())
			{
				for (long i = this.blob.Properties.Length; i > (long)0; i = i - (long)num)
				{
					num = (int)Math.Min(i, (long)this.manager.TransferOptions.BlockSize);
					BlobDownloadController.BlockData blockDatum = new BlobDownloadController.BlockData()
					{
						StartOffset = num1,
						Length = num,
						BlockName = num1.ToString()
					};
					this.blocksToDownload.Add(blockDatum);
					num1 = num1 + (long)num;
				}
			}
			else
			{
				foreach (ListBlockItem listBlockItem in listBlockItems)
				{
					int length = (int)listBlockItem.Length;
					BlobDownloadController.BlockData blockDatum1 = new BlobDownloadController.BlockData()
					{
						StartOffset = num1,
						Length = length,
						BlockName = listBlockItem.Name
					};
					this.blocksToDownload.Add(blockDatum1);
					num1 = num1 + (long)length;
				}
			}
			if (this.GetBlockNextDownloadIndex(asyncState))
			{
				if (this.md5HashStream.FinishedSeparateMd5Calculator)
				{
					this.state = BlobDownloadController.State.DownloadBlockBlob;
				}
				else
				{
					this.state = BlobDownloadController.State.CalculateMD5;
				}
				if (this.blocksToDownload.Count == this.nextDownloadIndex)
				{
					this.toDownloadItemsCountdownEvent = new CountdownEvent(1);
					if (!this.md5HashStream.FinishedSeparateMd5Calculator)
					{
						this.HasWork = true;
					}
					this.SetTransferFinished(asyncState);
					return;
				}
				this.nextWriteIndex = this.nextDownloadIndex;
				this.toDownloadItemsCountdownEvent = new CountdownEvent(this.blocksToDownload.Count - this.nextWriteIndex);
				this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(this.blob.Properties.Length, this.manager.TransferOptions.Concurrency, new Action<double, double>(this.ProgressCallbackHandler), this.manager.GlobalDownloadSpeedTracker);
				this.transferStatusTracker.AddBytesTransferred(this.transferEntry.CheckPoint.EntryTransferOffset);
				this.HasWork = true;
				asyncState.CallFinish(this);
				return;
			}
		}

		private void DownloadPageRangeCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			BlobDownloadController.DownloadPageState asyncState = asyncResult.AsyncState as BlobDownloadController.DownloadPageState;
			try
			{
				this.blob.EndDownloadRangeToStream(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			this.availablePageRangeData.TryAdd(asyncState.StartOffset, asyncState.MemoryBuffer);
			asyncState.MemoryBuffer = null;
			this.SetPageRangeDownloadHasWork();
			asyncState.CallbackState.CallFinish(this);
			asyncState.Dispose();
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
				this.HandleFetchAttributesException(exception, asyncState);
				return;
			}
			if (this.blob.Properties.BlobType != BlobType.Unspecified)
			{
				if (string.IsNullOrEmpty(this.transferEntry.ETag))
				{
					if ((long)0 != this.transferEntry.CheckPoint.EntryTransferOffset)
					{
						this.SetErrorState(new InvalidOperationException(Resources.RestartableInfoCorruptedException), asyncState);
						return;
					}
					this.transferEntry.ETag = this.blob.Properties.ETag;
				}
				else if (!this.transferEntry.ETag.Equals(this.blob.Properties.ETag, StringComparison.InvariantCultureIgnoreCase))
				{
					if (!this.RetransferModifiedCallbackHandler(this.blob.Name, asyncState))
					{
						return;
					}
					this.transferEntry.ETag = this.blob.Properties.ETag;
					this.transferEntry.CheckPoint.Clear();
				}
				else if (this.transferEntry.CheckPoint.EntryTransferOffset > this.blob.Properties.Length || this.transferEntry.CheckPoint.EntryTransferOffset < (long)0)
				{
					this.SetErrorState(new InvalidOperationException(Resources.RestartableInfoCorruptedException), asyncState);
					return;
				}
				this.md5HashStream = new MD5HashStream(this.outputStream, this.transferEntry.CheckPoint.EntryTransferOffset, this.checkMd5);
				this.ProgressCallbackHandler(0, 0);
				try
				{
					this.cancellationHandler.CheckCancellation();
					if (this.outputStream.Length != this.blob.Properties.Length)
					{
						this.outputStream.SetLength(this.blob.Properties.Length);
					}
				}
				catch (System.Exception exception1)
				{
					this.SetErrorState(exception1, asyncState);
					return;
				}
				if (this.blob.Properties.BlobType == BlobType.PageBlob)
				{
					this.pageBlob = this.blob as CloudPageBlob;
					this.state = BlobDownloadController.State.GetPageRanges;
					this.PrepareToGetPageRanges();
					if (this.getPageRangesSpanCount == 0)
					{
						this.InitPageBlobDownloadInfo(asyncState);
						return;
					}
				}
				else if (this.blob.Properties.BlobType == BlobType.BlockBlob)
				{
					this.blockBlob = this.blob as CloudBlockBlob;
					this.state = BlobDownloadController.State.DownloadBlockList;
				}
				this.HasWork = true;
				asyncState.CallFinish(this);
				return;
			}
			else
			{
				this.SetErrorState(new InvalidOperationException(Resources.FailedToGetBlobTypeException), asyncState);
			}
		}

		~BlobDownloadController()
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
			catch (System.Exception)
			{
			}
		}

		private bool GetBlockNextDownloadIndex(CallbackState callbackState)
		{
			return this.GetNextDownloadIndex(() => {
				if ((long)0 == this.transferEntry.CheckPoint.EntryTransferOffset)
				{
					this.nextDownloadIndex = 0;
					return;
				}
				if (this.blob.Properties.Length == this.transferEntry.CheckPoint.EntryTransferOffset)
				{
					this.nextDownloadIndex = this.blocksToDownload.Count;
					return;
				}
				this.nextDownloadIndex = this.blocksToDownload.BinarySearch(new BlobDownloadController.BlockData()
				{
					StartOffset = this.transferEntry.CheckPoint.EntryTransferOffset
				}, new BlobDownloadController.BlockDataComparer());
			}, callbackState);
		}

		private Action<Action<ITransferController>> GetCalculateMD5Action()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				if (this.blob.Properties.BlobType == BlobType.PageBlob)
				{
					this.pageBlob = this.blob as CloudPageBlob;
					this.state = BlobDownloadController.State.DownloadPageBlob;
				}
				else if (this.blob.Properties.BlobType == BlobType.BlockBlob)
				{
					this.blockBlob = this.blob as CloudBlockBlob;
					this.state = BlobDownloadController.State.DownloadBlockBlob;
				}
				this.HasWork = true;
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

		private Action<Action<ITransferController>> GetDeleteSourceAction()
		{
			this.HasWork = false;
			bool flag = false;
			if (this.transferEntry.BlobSet == null)
			{
				flag = true;
			}
			else if (this.transferEntry.BlobSet.CountDown.Signal())
			{
				flag = true;
				ICloudBlob rootBlob = this.transferEntry.BlobSet.RootBlob;
				if (!BlobExtensions.Equals(this.blob, rootBlob))
				{
					this.blob = rootBlob;
				}
			}
			else if (this.blob.SnapshotTime.HasValue)
			{
				flag = true;
			}
			if (flag)
			{
				return (Action<ITransferController> finishDelegate) => {
					CallbackState callbackState = new CallbackState()
					{
						FinishDelegate = finishDelegate
					};
					try
					{
						this.StartCallbackHandler();
						this.ProgressCallbackHandler(0, 100);
					}
					catch (System.Exception exception)
					{
						this.SetErrorState(exception, callbackState);
						return;
					}
					BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.Delete);
					OperationContext operationContext = new OperationContext();
					operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
					OperationContext operationContext1 = operationContext;
					DeleteSnapshotsOption deleteSnapshotsOption = (this.blob.SnapshotTime.HasValue ? DeleteSnapshotsOption.None : DeleteSnapshotsOption.IncludeSnapshots);
					try
					{
						this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginDelete(deleteSnapshotsOption, null, blobRequestOptions, operationContext1, new AsyncCallback(this.DeleteSourceCallback), callbackState));
					}
					catch (System.Exception exception1)
					{
						this.SetErrorState(exception1, callbackState);
					}
				};
			}
			return (Action<ITransferController> finishDelegate) => {
				this.ChangeStatus();
				finishDelegate(this);
			};
		}

		private Action<Action<ITransferController>> GetDownloadBlockBlobAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				BlobDownloadController.BlockCacheEntry blockCacheEntry;
				BlobDownloadController.BlockCacheEntry blockCacheEntry1;
				if (!this.hasActiveFileWriter && this.nextWriteIndex < this.blocksToDownload.Count)
				{
					string blockName = this.blocksToDownload[this.nextWriteIndex].BlockName;
					if (this.availableBlockData.TryGetValue(blockName, out blockCacheEntry) && blockCacheEntry.MemoryBuffer != null)
					{
						BlobDownloadController.DownloadBlockState downloadBlockState = new BlobDownloadController.DownloadBlockState()
						{
							BlockData = this.blocksToDownload[this.nextWriteIndex],
							CallbackState = new CallbackState()
							{
								FinishDelegate = finishDelegate
							},
							MemoryBuffer = blockCacheEntry.MemoryBuffer,
							BlockCacheEntry = blockCacheEntry
						};
						this.hasActiveFileWriter = true;
						BlobDownloadController blobDownloadController = this;
						blobDownloadController.nextWriteIndex = blobDownloadController.nextWriteIndex + 1;
						this.SetBlockDownloadHasWork();
						this.BeginWriteBlockData(downloadBlockState);
						return;
					}
				}
				if (this.nextDownloadIndex < this.blocksToDownload.Count)
				{
					BlobDownloadController.BlockData item = this.blocksToDownload[this.nextDownloadIndex];
					bool flag = false;
					if (this.availableBlockData.TryGetValue(item.BlockName, out blockCacheEntry1) && blockCacheEntry1.TryGetReference())
					{
						flag = true;
						BlobDownloadController blobDownloadController1 = this;
						blobDownloadController1.nextDownloadIndex = blobDownloadController1.nextDownloadIndex + 1;
					}
					if (!flag)
					{
						byte[] numArray = this.manager.MemoryManager.RequireBuffer();
						if (numArray != null)
						{
							blockCacheEntry1 = new BlobDownloadController.BlockCacheEntry(this, item.BlockName);
							if (this.availableBlockData.TryAdd(item.BlockName, blockCacheEntry1))
							{
								Interlocked.Add(ref this.reservedMemory, (long)item.Length);
								BlobDownloadController blobDownloadController2 = this;
								blobDownloadController2.nextDownloadIndex = blobDownloadController2.nextDownloadIndex + 1;
								BlobDownloadController.DownloadBlockState downloadBlockState1 = new BlobDownloadController.DownloadBlockState()
								{
									BlockData = item,
									CallbackState = new CallbackState()
									{
										FinishDelegate = finishDelegate
									},
									MemoryBuffer = numArray,
									BlockCacheEntry = blockCacheEntry1
								};
								this.SetBlockDownloadHasWork();
								this.BeginDownloadBlock(downloadBlockState1);
								return;
							}
						}
					}
				}
				this.SetBlockDownloadHasWork();
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
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blockBlob.BeginDownloadBlockList(0, accessCondition, blobRequestOptions, operationContext1, new AsyncCallback(this.DownloadBlockListCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
				}
			};
		}

		private Action<Action<ITransferController>> GetDownloadPageBlobAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				byte[] numArray;
				if (!this.hasActiveFileWriter && this.nextWriteIndex < this.pageRangeList.Count)
				{
					long startOffset = this.pageRangeList[this.nextWriteIndex].StartOffset;
					if (this.availablePageRangeData.TryGetValue(startOffset, out numArray))
					{
						BlobDownloadController.DownloadPageState downloadPageState = new BlobDownloadController.DownloadPageState()
						{
							PageRange = this.pageRangeList[this.nextWriteIndex],
							CallbackState = new CallbackState()
							{
								FinishDelegate = finishDelegate
							},
							MemoryBuffer = numArray
						};
						this.hasActiveFileWriter = true;
						BlobDownloadController blobDownloadController = this;
						blobDownloadController.nextWriteIndex = blobDownloadController.nextWriteIndex + 1;
						this.SetPageRangeDownloadHasWork();
						this.BeginWritePageRangeData(downloadPageState);
						return;
					}
				}
				if (this.nextDownloadIndex < this.pageRangeList.Count)
				{
					BlobDownloadController.PageBlobRange item = this.pageRangeList[this.nextDownloadIndex];
					byte[] numArray1 = this.manager.MemoryManager.RequireBuffer();
					if (numArray1 != null)
					{
						Interlocked.Add(ref this.reservedMemory, (long)((int)numArray1.Length));
						BlobDownloadController blobDownloadController1 = this;
						blobDownloadController1.nextDownloadIndex = blobDownloadController1.nextDownloadIndex + 1;
						BlobDownloadController.DownloadPageState downloadPageState1 = new BlobDownloadController.DownloadPageState()
						{
							PageRange = item,
							CallbackState = new CallbackState()
							{
								FinishDelegate = finishDelegate
							},
							MemoryBuffer = numArray1
						};
						this.SetPageRangeDownloadHasWork();
						this.BeginDownloadPageRange(downloadPageState1);
						return;
					}
				}
				this.SetPageRangeDownloadHasWork();
				finishDelegate(this);
			};
		}

		private Action<Action<ITransferController>> GetFetchAttributesAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					this.StartCallbackHandler();
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
					return;
				}
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.FetchAttributes);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.blob.BeginFetchAttributes(null, blobRequestOptions, operationContext1, new AsyncCallback(this.FetchAttributesCallback), callbackState));
				}
				catch (System.Exception exception1)
				{
					this.HandleFetchAttributesException(exception1, callbackState);
				}
			};
		}

		private bool GetNextDownloadIndex(Action binarySearch, CallbackState callbackState)
		{
			binarySearch();
			if (this.nextDownloadIndex >= 0)
			{
				return true;
			}
			this.SetErrorState(new InvalidOperationException(Resources.RestartableInfoCorruptedException), callbackState);
			return false;
		}

		private Action<Action<ITransferController>> GetOpenOutputStreamAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					this.StartCallbackHandler();
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, callbackState);
					return;
				}
				if (this.manager.TransferOptions.OverwritePromptCallback == null || !File.Exists(this.fileName) || this.OverwritePromptCallbackHandler(this.fileName, callbackState))
				{
					try
					{
						this.cancellationHandler.CheckCancellation();
						this.outputStream = new FileStream(this.fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
					}
					catch (OperationCanceledException operationCanceledException)
					{
						this.SetErrorState(operationCanceledException, callbackState);
						return;
					}
					catch (System.Exception exception1)
					{
						this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.OpenFileFailed, string.Format(CultureInfo.InvariantCulture, Resources.FailedToOpenFileException, new object[] { this.fileName }), exception1), callbackState);
						return;
					}
					this.state = BlobDownloadController.State.FetchAttributes;
					this.HasWork = true;
					finishDelegate(this);
					return;
				}
			};
		}

		private bool GetPageNextDownloadIndex(CallbackState callbackState)
		{
			return this.GetNextDownloadIndex(() => {
				if ((long)0 == this.transferEntry.CheckPoint.EntryTransferOffset)
				{
					this.nextDownloadIndex = 0;
					return;
				}
				if (this.blob.Properties.Length == this.transferEntry.CheckPoint.EntryTransferOffset)
				{
					this.nextDownloadIndex = this.pageRangeList.Count;
					return;
				}
				this.nextDownloadIndex = this.pageRangeList.BinarySearch(new BlobDownloadController.PageBlobRange()
				{
					StartOffset = this.transferEntry.CheckPoint.EntryTransferOffset
				}, new BlobDownloadController.PageBlobRangeComparer());
			}, callbackState);
		}

		private Action<Action<ITransferController>> GetPageRangesAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				int num = Interlocked.Increment(ref this.getPageRangesSpanIndex);
				this.HasWork = num < this.getPageRangesSpanCount - 1;
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.GetPageRanges);
				AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.pageBlob.Properties.ETag);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				BlobDownloadController.PageRangesSpan pageRangesSpan = new BlobDownloadController.PageRangesSpan()
				{
					StartOffset = (long)num * (long)155189248,
					CallbackState = new CallbackState()
					{
						FinishDelegate = finishDelegate
					},
					
				};

                pageRangesSpan.EndOffset = Math.Min(this.blob.Properties.Length, pageRangesSpan.StartOffset + (long)155189248) - (long)1;

				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.pageBlob.BeginGetPageRanges(new long?(pageRangesSpan.StartOffset), new long?(pageRangesSpan.EndOffset - pageRangesSpan.StartOffset + (long)1), accessCondition, blobRequestOptions, operationContext1, new AsyncCallback(this.GetPageRangesCallback), pageRangesSpan));
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(exception, pageRangesSpan.CallbackState);
				}
			};
		}

		private void GetPageRangesCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			BlobDownloadController.PageRangesSpan asyncState = asyncResult.AsyncState as BlobDownloadController.PageRangesSpan;
			try
			{
				asyncState.PageRanges = new List<PageRange>(this.pageBlob.EndGetPageRanges(asyncResult));
				lock (this.getPageRangesLock)
				{
					this.pageRangesSpanSet.Add(asyncState);
				}
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState.CallbackState);
				return;
			}
			if (this.getPageRangesCountDownEvent.Signal())
			{
				this.ArrangePageRanges();
				this.InitPageBlobDownloadInfo(asyncState.CallbackState);
				return;
			}
			asyncState.CallbackState.CallFinish(this);
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
				case (BlobDownloadController.State)BlobDownloadController.State.OpenOutputStream:
				{
					return this.GetOpenOutputStreamAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.FetchAttributes:
				{
					return this.GetFetchAttributesAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.DownloadBlockList:
				{
					return this.GetDownloadBlockListAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.CalculateMD5:
				{
					return this.GetCalculateMD5Action();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.DownloadBlockBlob:
				{
					return this.GetDownloadBlockBlobAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.GetPageRanges:
				{
					return this.GetPageRangesAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.DownloadPageBlob:
				{
					return this.GetDownloadPageBlobAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.DeleteSource:
				{
					return this.GetDeleteSourceAction();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.Finished:
				{
					throw new InvalidOperationException();
				}
				case (BlobDownloadController.State)BlobDownloadController.State.Error:
				{
					throw new InvalidOperationException();
				}
			}
			throw new InvalidOperationException();
		}

		private void HandleFetchAttributesException(System.Exception e, CallbackState callbackState)
		{
			StorageException storageException = e as StorageException;
			if (storageException == null)
			{
				this.SetErrorState(e, callbackState);
				return;
			}
			if (storageException.RequestInformation == null || storageException.RequestInformation.HttpStatusCode != 404)
			{
				this.SetErrorState(storageException, callbackState);
				return;
			}
			this.SetErrorState(new InvalidOperationException(Resources.SourceBlobDoesNotExistException), callbackState);
		}

		private void InitPageBlobDownloadInfo(CallbackState callbackState)
		{
			this.ClearForGetPageRanges();
			if (!this.GetPageNextDownloadIndex(callbackState))
			{
				return;
			}
			if (this.md5HashStream.FinishedSeparateMd5Calculator)
			{
				this.state = BlobDownloadController.State.DownloadPageBlob;
			}
			else
			{
				this.state = BlobDownloadController.State.CalculateMD5;
			}
			if (this.pageRangeList.Count == this.nextDownloadIndex)
			{
				this.toDownloadItemsCountdownEvent = new CountdownEvent(1);
				if (!this.md5HashStream.FinishedSeparateMd5Calculator)
				{
					this.HasWork = true;
				}
				this.SetTransferFinished(callbackState);
				return;
			}
			this.nextWriteIndex = this.nextDownloadIndex;
			this.toDownloadItemsCountdownEvent = new CountdownEvent(this.pageRangeList.Count - this.nextDownloadIndex);
			this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(this.blob.Properties.Length, this.manager.TransferOptions.Concurrency, new Action<double, double>(this.ProgressCallbackHandler), this.manager.GlobalDownloadSpeedTracker);
			this.transferStatusTracker.AddBytesTransferred(this.transferEntry.CheckPoint.EntryTransferOffset);
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

		private void PrepareToGetPageRanges()
		{
			this.getPageRangesSpanCount = (int)Math.Ceiling((double)this.blob.Properties.Length / 155189248);
			this.getPageRangesCountDownEvent = new CountdownEvent(this.getPageRangesSpanCount);
			this.getPageRangesLock = new object();
			this.getPageRangesSpanIndex = -1;
			this.pageRangesSpanSet = new HashSet<BlobDownloadController.PageRangesSpan>();
			this.pageRangeList = new List<BlobDownloadController.PageBlobRange>();
			this.availablePageRangeData = new ConcurrentDictionary<long, byte[]>();
			this.nextDownloadIndex = 0;
			
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

		private bool PromptCallBackExceptionHandler(BlobDownloadController.PromptCallbackAction promptcallbackAction, CallbackState callbackState)
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

		private void SetBlockDownloadHasWork()
		{
			BlobDownloadController.BlockCacheEntry blockCacheEntry;
			if (this.HasWork)
			{
				return;
			}
			int num = this.nextWriteIndex;
			if (!this.hasActiveFileWriter && num < this.blocksToDownload.Count)
			{
				string blockName = this.blocksToDownload[num].BlockName;
				if (this.availableBlockData.TryGetValue(blockName, out blockCacheEntry))
				{
					this.HasWork = true;
					return;
				}
			}
			if (this.nextDownloadIndex < this.blocksToDownload.Count)
			{
				this.HasWork = true;
			}
		}

		private void SetErrorState(System.Exception ex, DownloadDataState downloadState)
		{
			this.SetErrorState(ex, downloadState.CallbackState);
			downloadState.Dispose();
		}

		private void SetErrorState(System.Exception ex, CallbackState callbackState)
		{
			lock (this.exceptionListLock)
			{
				this.exceptionList.Add(ex);
			}
			this.state = BlobDownloadController.State.Error;
			this.HasWork = false;
			this.IsFinished = true;
			if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
			{
				this.CloseOwnedOutputStream();
			}
			this.FinishCallbackHandler(ex);
			callbackState.CallFinish(this);
		}

		private void SetFinished()
		{
			this.state = BlobDownloadController.State.Finished;
			this.HasWork = false;
			this.IsFinished = true;
			this.FinishCallbackHandler(null);
		}

		private void SetHasWorkAfterStatusChanged()
		{
			if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
			{
				if (this.outputStream != null)
				{
					if (!this.outputStream.CanWrite)
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						string streamMustSupportWriteException = Resources.StreamMustSupportWriteException;
						object[] objArray = new object[] { "outputStream" };
						throw new NotSupportedException(string.Format(invariantCulture, streamMustSupportWriteException, objArray));
					}
					if (!this.outputStream.CanSeek)
					{
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						string streamMustSupportSeekException = Resources.StreamMustSupportSeekException;
						object[] objArray1 = new object[] { "outputStream" };
						throw new NotSupportedException(string.Format(cultureInfo, streamMustSupportSeekException, objArray1));
					}
					this.state = BlobDownloadController.State.FetchAttributes;
				}
				else
				{
					this.state = BlobDownloadController.State.OpenOutputStream;
				}
			}
			else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
			{
				this.state = BlobDownloadController.State.DeleteSource;
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

		private void SetPageRangeDownloadHasWork()
		{
			byte[] numArray;
			if (this.HasWork)
			{
				return;
			}
			int num = this.nextWriteIndex;
			if (!this.hasActiveFileWriter && num < this.pageRangeList.Count)
			{
				long startOffset = this.pageRangeList[num].StartOffset;
				if (this.availablePageRangeData.TryGetValue(startOffset, out numArray))
				{
					this.HasWork = true;
					return;
				}
			}
			if (this.nextDownloadIndex < this.pageRangeList.Count)
			{
				this.HasWork = true;
			}
		}

		private void SetTransferFinished(DownloadDataState downloadState)
		{
			this.SetTransferFinished(downloadState.CallbackState);
			downloadState.Dispose();
		}

		private void SetTransferFinished(CallbackState callbackState)
		{
			if (this.toDownloadItemsCountdownEvent.Signal())
			{
				if (this.md5HashStream.CheckMd5Hash)
				{
					this.md5HashStream.MD5HashTransformFinalBlock(new byte[0], 0, 0);
					string base64String = Convert.ToBase64String(this.md5HashStream.Hash);
					string contentMD5 = this.blob.Properties.ContentMD5;
					if (!base64String.Equals(contentMD5))
					{
						this.SetErrorState(new InvalidOperationException(string.Format(Resources.DownloadedMd5MismatchException, base64String, contentMD5)), callbackState);
						return;
					}
				}
				this.CloseOwnedOutputStream();
				this.ChangeStatus();
			}
			callbackState.CallFinish(this);
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
			if (this.state == BlobDownloadController.State.Error)
			{
				throw this.Exception;
			}
		}

		private void WriteBlockDataCallback(IAsyncResult asyncResult)
		{
			BlobDownloadController.DownloadBlockState asyncState = asyncResult.AsyncState as BlobDownloadController.DownloadBlockState;
			try
			{
				this.md5HashStream.EndWrite(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			lock (this.transferEntry.EntryLock)
			{
				BlobTransferFileTransferEntryCheckPoint checkPoint = this.transferEntry.CheckPoint;
				checkPoint.EntryTransferOffset = checkPoint.EntryTransferOffset + (long)asyncState.Length;
			}
			if (this.md5HashStream.MD5HashTransformBlock(asyncState.StartOffset, asyncState.MemoryBuffer, 0, asyncState.Length, null, 0))
			{
				try
				{
					this.transferStatusTracker.AddBytesTransferred((long)asyncState.Length);
				}
				catch (BlobTransferCallbackException blobTransferCallbackException)
				{
					this.SetErrorState(blobTransferCallbackException, asyncState);
					return;
				}
				asyncState.BlockCacheEntry.DecrementRefCount();
				this.hasActiveFileWriter = false;
				this.SetBlockDownloadHasWork();
				this.SetTransferFinished(asyncState);
				return;
			}
		}

		private void WritePageRangeDataCallback(IAsyncResult asyncResult)
		{
			byte[] numArray;
			BlobDownloadController.DownloadPageState asyncState = asyncResult.AsyncState as BlobDownloadController.DownloadPageState;
			try
			{
				this.md5HashStream.EndWrite(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(exception, asyncState);
				return;
			}
			lock (this.transferEntry.EntryLock)
			{
				BlobTransferFileTransferEntryCheckPoint checkPoint = this.transferEntry.CheckPoint;
				checkPoint.EntryTransferOffset = checkPoint.EntryTransferOffset + (long)asyncState.Length;
			}
			if (this.md5HashStream.MD5HashTransformBlock(asyncState.StartOffset, asyncState.MemoryBuffer, 0, asyncState.Length, null, 0))
			{
				try
				{
					this.transferStatusTracker.AddBytesTransferred((long)asyncState.Length);
				}
				catch (BlobTransferCallbackException blobTransferCallbackException)
				{
					this.SetErrorState(blobTransferCallbackException, asyncState);
					return;
				}
				this.availablePageRangeData.TryRemove(asyncState.StartOffset, out numArray);
				Interlocked.Add(ref this.reservedMemory, (long)(-(int)asyncState.MemoryBuffer.Length));
				this.manager.MemoryManager.ReleaseBuffer(asyncState.MemoryBuffer);
				this.hasActiveFileWriter = false;
				this.SetPageRangeDownloadHasWork();
				this.SetTransferFinished(asyncState);
				return;
			}
		}

		private class BlockCacheEntry
		{
			private BlobDownloadController downloadController;

			private string blockName;

			private int refCount;

			private object removeLockObject = new object();

			public byte[] MemoryBuffer
			{
				get;
				set;
			}

			public BlockCacheEntry(BlobDownloadController downloadController, string blockName)
			{
				this.downloadController = downloadController;
				this.blockName = blockName;
				this.refCount = 1;
			}

			public void DecrementRefCount()
			{
				BlobDownloadController.BlockCacheEntry blockCacheEntry;
				if (Interlocked.Decrement(ref this.refCount) == 0)
				{
					lock (this.removeLockObject)
					{
						if (this.refCount == 0)
						{
							this.downloadController.availableBlockData.TryRemove(this.blockName, out blockCacheEntry);
							Interlocked.Add(ref this.downloadController.reservedMemory, (long)(-(int)this.MemoryBuffer.Length));
							this.downloadController.manager.MemoryManager.ReleaseBuffer(this.MemoryBuffer);
							this.MemoryBuffer = null;
						}
					}
				}
			}

			public bool TryGetReference()
			{
				bool flag;
				if (1 == Interlocked.Increment(ref this.refCount))
				{
					lock (this.removeLockObject)
					{
						if (this.MemoryBuffer != null)
						{
							return true;
						}
						else
						{
							flag = false;
						}
					}
					return flag;
				}
				return true;
			}
		}

		private class BlockData
		{
			public string BlockName
			{
				get;
				set;
			}

			public int Length
			{
				get;
				set;
			}

			public long StartOffset
			{
				get;
				set;
			}

			public BlockData()
			{
			}
		}

		private class BlockDataComparer : IComparer<BlobDownloadController.BlockData>
		{
			public BlockDataComparer()
			{
			}

			public int Compare(BlobDownloadController.BlockData x, BlobDownloadController.BlockData y)
			{
				return Math.Sign(x.StartOffset - y.StartOffset);
			}
		}

		private class DownloadBlockState : DownloadDataState
		{
			private BlobDownloadController.BlockData blockData;

			public BlobDownloadController.BlockCacheEntry BlockCacheEntry
			{
				get;
				set;
			}

			public BlobDownloadController.BlockData BlockData
			{
				get
				{
					return this.blockData;
				}
				set
				{
					this.blockData = value;
					base.StartOffset = value.StartOffset;
					base.Length = value.Length;
				}
			}

			public DownloadBlockState()
			{
			}
		}

		private class DownloadPageState : DownloadDataState
		{
			private BlobDownloadController.PageBlobRange pageRange;

			public BlobDownloadController.PageBlobRange PageRange
			{
				get
				{
					return this.pageRange;
				}
				set
				{
					this.pageRange = value;
					base.StartOffset = value.StartOffset;
					base.Length = (int)(value.EndOffset - value.StartOffset + (long)1);
				}
			}

			public DownloadPageState()
			{
			}
		}

		private class PageBlobRange
		{
			public long EndOffset
			{
				get;
				set;
			}

			public bool HasData
			{
				get;
				set;
			}

			public long StartOffset
			{
				get;
				set;
			}

			public PageBlobRange()
			{
			}

			public IEnumerable<BlobDownloadController.PageBlobRange> SplitRanges(long maxPageRangeSize)
			{
				long num = this.StartOffset;
				long endOffset = this.EndOffset - this.StartOffset + (long)1;
				do
				{
					BlobDownloadController.PageBlobRange pageBlobRange = new BlobDownloadController.PageBlobRange()
					{
						StartOffset = num,
						EndOffset = num + Math.Min(endOffset, maxPageRangeSize) - (long)1,
						HasData = this.HasData
					};
					BlobDownloadController.PageBlobRange pageBlobRange1 = pageBlobRange;
					num = num + maxPageRangeSize;
					endOffset = endOffset - maxPageRangeSize;
					yield return pageBlobRange1;
				}
				while (endOffset > (long)0);
			}
		}

		private class PageBlobRangeComparer : IComparer<BlobDownloadController.PageBlobRange>
		{
			public PageBlobRangeComparer()
			{
			}

			public int Compare(BlobDownloadController.PageBlobRange x, BlobDownloadController.PageBlobRange y)
			{
				return Math.Sign(x.StartOffset - y.StartOffset);
			}
		}

		private class PageRangesSpan
		{
			public CallbackState CallbackState
			{
				get;
				set;
			}

			public long EndOffset
			{
				get;
				set;
			}

			public List<PageRange> PageRanges
			{
				get;
				set;
			}

			public long StartOffset
			{
				get;
				set;
			}

			public PageRangesSpan()
			{
			}
		}

		private delegate bool PromptCallbackAction();

		private enum State
		{
			OpenOutputStream,
			FetchAttributes,
			DownloadBlockList,
			CalculateMD5,
			DownloadBlockBlob,
			GetPageRanges,
			DownloadPageBlob,
			DeleteSource,
			Finished,
			Error
		}
	}
}