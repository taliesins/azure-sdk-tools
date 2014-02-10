using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public sealed class BlobTransferManager : IDisposable
	{
		private BlockingCollection<ITransferController> controllerQueue;

		private BlockingCollection<ITransferController> monitorQueue;

		private ConcurrentQueue<ITransferController> internalControllerQueue;

		private ConcurrentQueue<ITransferController> internalMonitorQueue;

		private CountdownEvent controllerAddersCountdownEvent;

		private CountdownEvent monitorAddersCountdownEvent;

		private int hasSignaledControllerAdders;

		private int hasSignaledMonitorAdders;

		private ConcurrentDictionary<ITransferController, object> activeControllerItems = new ConcurrentDictionary<ITransferController, object>();

		private ConcurrentDictionary<ITransferController, object> activeMonitorItems = new ConcurrentDictionary<ITransferController, object>();

		private System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource();

		private BlobTransferOptions transferOptions;

		private volatile int activeTasks;

		private ManualResetEventSlim controllerResetEvent = new ManualResetEventSlim();

		private Microsoft.WindowsAzure.Storage.DataMovement.MemoryManager memoryManager;

		private Random randomGenerator;

		private CancellationChecker cancellationChecker = new CancellationChecker();

		private CancellationTokenRegistration cancellationTokenRegistration;

		private TransferSpeedTracker globalDownloadSpeedTracker;

		private TransferSpeedTracker globalUploadSpeedTracker;

		private TransferSpeedTracker globalCopySpeedTracker;

		internal System.Threading.CancellationTokenSource CancellationTokenSource
		{
			get
			{
				return this.cancellationTokenSource;
			}
		}

		internal TransferSpeedTracker GlobalCopySpeedTracker
		{
			get
			{
				return this.globalCopySpeedTracker;
			}
		}

		internal TransferSpeedTracker GlobalDownloadSpeedTracker
		{
			get
			{
				return this.globalDownloadSpeedTracker;
			}
		}

		internal TransferSpeedTracker GlobalUploadSpeedTracker
		{
			get
			{
				return this.globalUploadSpeedTracker;
			}
		}

		internal Microsoft.WindowsAzure.Storage.DataMovement.MemoryManager MemoryManager
		{
			get
			{
				return this.memoryManager;
			}
		}

		public int QueuedItemsCount
		{
			get
			{
				return this.controllerQueue.Count + this.monitorQueue.Count;
			}
		}

		internal BlobTransferOptions TransferOptions
		{
			get
			{
				return this.transferOptions;
			}
		}

		public BlobTransferManager() : this(null)
		{
		}

		public BlobTransferManager(BlobTransferOptions options)
		{
			this.transferOptions = options ?? new BlobTransferOptions();
			this.globalDownloadSpeedTracker = new TransferSpeedTracker(new Action<double>(this.OnGlobalDownloadSpeed), this.transferOptions.Concurrency);
			this.globalUploadSpeedTracker = new TransferSpeedTracker(new Action<double>(this.OnGlobalUploadSpeed), this.transferOptions.Concurrency);
			this.globalCopySpeedTracker = new TransferSpeedTracker(new Action<double>(this.OnGlobalCopySpeed), this.transferOptions.Concurrency);
			this.internalControllerQueue = new ConcurrentQueue<ITransferController>();
			this.internalMonitorQueue = new ConcurrentQueue<ITransferController>();
			this.controllerQueue = new BlockingCollection<ITransferController>(this.internalControllerQueue);
			this.monitorQueue = new BlockingCollection<ITransferController>(this.internalMonitorQueue);
			this.controllerAddersCountdownEvent = new CountdownEvent(1);
			this.monitorAddersCountdownEvent = new CountdownEvent(1);
			this.hasSignaledControllerAdders = 0;
			this.hasSignaledMonitorAdders = 0;
			
			this.memoryManager = new Microsoft.WindowsAzure.Storage.DataMovement.MemoryManager(this.transferOptions.MaximumCacheSize, this.transferOptions.BlockSize);
			this.randomGenerator = new Random();
			CancellationToken token = this.cancellationTokenSource.Token;
			this.cancellationTokenRegistration = token.Register(new Action(this.cancellationChecker.Cancel));
			this.StartControllerThread();
		}

		private void AddToQueue(ITransferController item, CancellationToken cancellationToken, bool addController)
		{
			if (item.CanAddController)
			{
				this.controllerAddersCountdownEvent.AddCount();
			}
			if (item.CanAddMonitor)
			{
				this.monitorAddersCountdownEvent.AddCount();
			}
			if (addController)
			{
				this.controllerQueue.Add(item, cancellationToken);
				return;
			}
			this.monitorQueue.Add(item, cancellationToken);
		}

		public void CancelWork()
		{
			this.cancellationTokenSource.Cancel();
		}

		public void CancelWorkAndWaitForCompletion()
		{
			this.CancelWork();
			this.WaitForCompletion();
		}

		private void ControllerThread()
		{
			SpinWait spinWait = new SpinWait();
			while (!this.cancellationTokenSource.Token.IsCancellationRequested && (!this.controllerQueue.IsCompleted || !this.monitorQueue.IsCompleted || this.activeControllerItems.Any<KeyValuePair<ITransferController, object>>() || this.activeMonitorItems.Any<KeyValuePair<ITransferController, object>>() || this.activeTasks > 0))
			{
				BlobTransferManager.FillInQueue(this.activeControllerItems, this.controllerQueue, this.internalControllerQueue, this.cancellationTokenSource.Token, this.transferOptions.Concurrency, spinWait);
				BlobTransferManager.FillInQueue(this.activeMonitorItems, this.monitorQueue, this.internalMonitorQueue, this.cancellationTokenSource.Token, this.transferOptions.Concurrency, spinWait);
				if (this.cancellationTokenSource.Token.IsCancellationRequested || this.activeTasks < this.transferOptions.Concurrency && (this.DoWorkFrom(this.activeControllerItems, spinWait) || this.DoWorkFrom(this.activeMonitorItems, spinWait)))
				{
					continue;
				}
				spinWait.SpinOnce();
			}
			if (this.cancellationTokenSource.Token.IsCancellationRequested)
			{
				foreach (KeyValuePair<ITransferController, object> activeControllerItem in this.activeControllerItems)
				{
					activeControllerItem.Key.CancelWork();
				}
				foreach (KeyValuePair<ITransferController, object> activeMonitorItem in this.activeMonitorItems)
				{
					activeMonitorItem.Key.CancelWork();
				}
			}
			spinWait.Reset();
			while (this.activeTasks != null)
			{
				spinWait.SpinOnce();
			}
			foreach (ITransferController transferController in this.activeControllerItems.Keys.Union<ITransferController>(this.activeMonitorItems.Keys))
			{
				IDisposable disposable = transferController as IDisposable;
				if (disposable == null)
				{
					continue;
				}
				disposable.Dispose();
			}
			this.controllerResetEvent.Set();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					this.cancellationTokenRegistration.Dispose();
				}
				catch (ObjectDisposedException objectDisposedException)
				{
				}
				if (this.controllerAddersCountdownEvent != null)
				{
					this.controllerAddersCountdownEvent.Dispose();
					this.controllerAddersCountdownEvent = null;
				}
				if (this.monitorAddersCountdownEvent != null)
				{
					this.monitorAddersCountdownEvent.Dispose();
					this.monitorAddersCountdownEvent = null;
				}
				if (this.controllerQueue != null)
				{
					this.controllerQueue.Dispose();
					this.controllerQueue = null;
				}
				if (this.monitorQueue != null)
				{
					this.monitorQueue.Dispose();
					this.monitorQueue = null;
				}
				if (this.cancellationTokenSource != null)
				{
					this.cancellationTokenSource.Dispose();
					this.cancellationTokenSource = null;
				}
				if (this.controllerResetEvent != null)
				{
					this.controllerResetEvent.Dispose();
					this.controllerResetEvent = null;
				}
			}
		}

		private bool DoWorkFrom(ConcurrentDictionary<ITransferController, object> activeItems, SpinWait sw)
		{
			List<KeyValuePair<ITransferController, object>> keyValuePairs = new List<KeyValuePair<ITransferController, object>>(activeItems.Where<KeyValuePair<ITransferController, object>>((KeyValuePair<ITransferController, object> item) => {
				if (!item.Key.HasWork)
				{
					return false;
				}
				return !item.Key.IsFinished;
			}));
			bool flag = false;
			if (keyValuePairs.Count != 0)
			{
				sw.Reset();
				int num = this.randomGenerator.Next(keyValuePairs.Count);
				Action<Action<ITransferController>> work = keyValuePairs[num].Key.GetWork();
				if (work != null)
				{
					flag = true;
					Interlocked.Increment(ref this.activeTasks);
					try
					{
						keyValuePairs[num].Key.PreWork();
						work(new Action<ITransferController>(this.FinishedWorkItem));
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						string.Format("work() should never throw an exception: {0}", exception.StackTrace);
						this.FinishWorkItem(keyValuePairs[num].Key, true, exception);
					}
				}
			}
			return flag;
		}

		private static void FillInQueue(ConcurrentDictionary<ITransferController, object> activeItems, BlockingCollection<ITransferController> collection, ConcurrentQueue<ITransferController> queueInCollection, CancellationToken token, int countUpperBound, SpinWait sw)
		{
			while (!token.IsCancellationRequested && activeItems.Count < countUpperBound && !queueInCollection.IsEmpty)
			{
				sw.Reset();
				ITransferController transferController = null;
				try
				{
					transferController = collection.Take(token);
				}
				catch (OperationCanceledException operationCanceledException)
				{
					continue;
				}
				catch (InvalidOperationException invalidOperationException)
				{
					continue;
				}
				activeItems.TryAdd(transferController, null);
			}
		}

		~BlobTransferManager()
		{
			this.Dispose(false);
		}

		private void FinishedWorkItem(ITransferController transferController)
		{
			this.FinishWorkItem(transferController, transferController.IsFinished, transferController.Exception);
		}

		private void FinishWorkItem(ITransferController transferController, bool finished, Exception exception)
		{
			object obj;
			if (transferController.PostWork() == 0 && finished)
			{
				if (!this.activeControllerItems.TryRemove(transferController, out obj))
				{
					this.activeMonitorItems.TryRemove(transferController, out obj);
				}
				if (transferController.CanAddController || transferController.CanAddMonitor)
				{
					this.SignalQueueAdders(true, transferController.CanAddController, transferController.CanAddMonitor);
				}
				IDisposable disposable = transferController as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			Interlocked.Decrement(ref this.activeTasks);
		}

		private void OnGlobalCopySpeed(double globalSpeed)
		{
			EventHandler<BlobTransferManagerEventArgs> eventHandler = this.GlobalCopySpeedUpdated;
			if (eventHandler != null)
			{
				eventHandler(this, new BlobTransferManagerEventArgs((globalSpeed < 0 ? 0 : globalSpeed)));
			}
		}

		private void OnGlobalDownloadSpeed(double globalSpeed)
		{
			EventHandler<BlobTransferManagerEventArgs> eventHandler = this.GlobalDownloadSpeedUpdated;
			if (eventHandler != null)
			{
				eventHandler(this, new BlobTransferManagerEventArgs((globalSpeed < 0 ? 0 : globalSpeed)));
			}
		}

		private void OnGlobalUploadSpeed(double globalSpeed)
		{
			EventHandler<BlobTransferManagerEventArgs> eventHandler = this.GlobalUploadSpeedUpdated;
			if (eventHandler != null)
			{
				eventHandler(this, new BlobTransferManagerEventArgs((globalSpeed < 0 ? 0 : globalSpeed)));
			}
		}

		public void QueueBlobCopy(Uri sourceUri, CloudBlobContainer destinationContainer, string destinationBlobName, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.QueueBlobCopy(new BlobTransferFileTransferEntry(), sourceUri, null, destinationContainer, destinationBlobName, false, startCallback, progressCallback, finishCallback, userData);
		}

		public void QueueBlobCopy(ICloudBlob sourceBlob, CloudBlobContainer destinationContainer, string destinationBlobName, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.QueueBlobCopy(new BlobTransferFileTransferEntry(), null, sourceBlob, destinationContainer, destinationBlobName, false, startCallback, progressCallback, finishCallback, userData);
		}

		internal void QueueBlobCopy(BlobTransferFileTransferEntry transferEntry, Uri sourceUri, ICloudBlob sourceBlob, CloudBlobContainer destinationContainer, string destinationBlobName, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.cancellationChecker.CheckCancellation();
			Action<object, string, Exception> action = null;
			if (finishCallback != null)
			{
				action = (object startCopyUserData, string copyId, Exception ex) => finishCallback(startCopyUserData, ex);
			}
			this.AddToQueue(new BlobStartCopyController(this, transferEntry, sourceUri, sourceBlob, destinationContainer, destinationBlobName, true, moveSource, startCallback, progressCallback, action, userData), this.cancellationTokenSource.Token, true);
		}

		internal void QueueBlobCopyMonitor(BlobTransferFileTransferEntry transferEntry, Uri sourceUri, ICloudBlob sourceBlob, ICloudBlob destinationBlob, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.cancellationChecker.CheckCancellation();
			this.AddToQueue(new BlobCopyMonitor(this, transferEntry, sourceUri, sourceBlob, destinationBlob, moveSource, startCallback, progressCallback, finishCallback, userData), this.cancellationTokenSource.Token, false);
		}

		public void QueueBlobStartCopy(Uri sourceUri, CloudBlobContainer destinationContainer, string destinationBlobName, Action<object> startCallback, Action<object, string, Exception> finishCallback, object userData)
		{
			this.QueueBlobStartCopy(new BlobTransferFileTransferEntry(), sourceUri, null, destinationContainer, destinationBlobName, startCallback, null, finishCallback, userData);
		}

		public void QueueBlobStartCopy(ICloudBlob sourceBlob, CloudBlobContainer destinationContainer, string destinationBlobName, Action<object> startCallback, Action<object, string, Exception> finishCallback, object userData)
		{
			this.QueueBlobStartCopy(new BlobTransferFileTransferEntry(), null, sourceBlob, destinationContainer, destinationBlobName, startCallback, null, finishCallback, userData);
		}

		internal void QueueBlobStartCopy(BlobTransferFileTransferEntry transferEntry, Uri sourceUri, ICloudBlob sourceBlob, CloudBlobContainer destinationContainer, string destinationBlobName, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, string, Exception> finishCallback, object userData)
		{
			this.cancellationChecker.CheckCancellation();
			this.AddToQueue(new BlobStartCopyController(this, transferEntry, sourceUri, sourceBlob, destinationContainer, destinationBlobName, false, false, startCallback, progressCallback, finishCallback, userData), this.cancellationTokenSource.Token, true);
		}

		public void QueueDownload(ICloudBlob blob, string fileName, bool checkMd5, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.QueueDownload(new BlobTransferFileTransferEntry(), blob, fileName, null, checkMd5, false, startCallback, progressCallback, finishCallback, userData);
		}

		public void QueueDownload(ICloudBlob blob, Stream outputStream, bool checkMd5, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.QueueDownload(new BlobTransferFileTransferEntry(), blob, null, outputStream, checkMd5, false, startCallback, progressCallback, finishCallback, userData);
		}

		internal void QueueDownload(BlobTransferFileTransferEntry transferEntry, ICloudBlob blob, string fileName, Stream outputStream, bool checkMd5, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.cancellationChecker.CheckCancellation();
			this.AddToQueue(new BlobDownloadController(this, transferEntry, blob, fileName, outputStream, checkMd5, moveSource, startCallback, progressCallback, finishCallback, userData), this.cancellationTokenSource.Token, true);
		}

		public void QueueRecursiveTransfer(string sourceLocation, string destinationLocation, BlobTransferRecursiveTransferOptions options, Action<object> startCallback, Action<object, Exception> finishCallback, Action<object, EntryData> startFileCallback, Action<object, EntryData, double, double> progressFileCallback, Action<object, EntryData, Exception> finishFileCallback, object userData)
		{
			this.cancellationChecker.CheckCancellation();
			this.AddToQueue(new BlobTransferRecursiveTransferItem(this, sourceLocation, destinationLocation, options, startCallback, finishCallback, startFileCallback, progressFileCallback, finishFileCallback, userData), this.cancellationTokenSource.Token, true);
		}

		public void QueueUpload(ICloudBlob blob, string fileName, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.QueueUpload(new BlobTransferFileTransferEntry(), blob, fileName, null, false, startCallback, progressCallback, finishCallback, userData);
		}

		public void QueueUpload(ICloudBlob blob, Stream inputStream, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.QueueUpload(new BlobTransferFileTransferEntry(), blob, null, inputStream, false, startCallback, progressCallback, finishCallback, userData);
		}

		internal void QueueUpload(BlobTransferFileTransferEntry transferEntry, ICloudBlob blob, string fileName, Stream inputStream, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, Exception> finishCallback, object userData)
		{
			this.cancellationChecker.CheckCancellation();
			if (BlobType.PageBlob == blob.BlobType)
			{
				this.AddToQueue(new PageBlobUploadController(this, transferEntry, blob as CloudPageBlob, fileName, inputStream, moveSource, startCallback, progressCallback, finishCallback, userData), this.cancellationTokenSource.Token, true);
				return;
			}
			if (BlobType.BlockBlob != blob.BlobType)
			{
				throw new InvalidOperationException(Resources.OnlySupportTwoBlobTypesException);
			}
			this.AddToQueue(new BlockBlobUploadController(this, transferEntry, blob as CloudBlockBlob, fileName, inputStream, moveSource, startCallback, progressCallback, finishCallback, userData), this.cancellationTokenSource.Token, true);
		}

		private void SignalQueueAdders(bool alwaysSignal, bool removeControllerAdder, bool removeMonitorAdder)
		{
			if (removeControllerAdder && (alwaysSignal || Interlocked.CompareExchange(ref this.hasSignaledControllerAdders, 1, 0) == 0) && this.controllerAddersCountdownEvent.Signal())
			{
				this.controllerQueue.CompleteAdding();
			}
			if (removeMonitorAdder && (alwaysSignal || Interlocked.CompareExchange(ref this.hasSignaledMonitorAdders, 1, 0) == 0) && this.monitorAddersCountdownEvent.Signal())
			{
				this.monitorQueue.CompleteAdding();
			}
		}

		private void StartControllerThread()
		{
			ThreadPool.QueueUserWorkItem((object param0) => this.ControllerThread());
		}

		public void WaitForCompletion()
		{
			this.SignalQueueAdders(false, true, true);
			this.controllerResetEvent.Wait();
		}

		public event EventHandler<BlobTransferManagerEventArgs> GlobalCopySpeedUpdated;

		public event EventHandler<BlobTransferManagerEventArgs> GlobalDownloadSpeedUpdated;

		public event EventHandler<BlobTransferManagerEventArgs> GlobalUploadSpeedUpdated;
	}
}