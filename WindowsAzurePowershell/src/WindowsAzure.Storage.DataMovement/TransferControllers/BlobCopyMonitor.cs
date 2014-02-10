using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using Microsoft.WindowsAzure.Storage.DataMovement.Extensions;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal class BlobCopyMonitor : IDisposable, ITransferController
	{
		private AsyncCallCancellationHandler cancellationHandler = new AsyncCallCancellationHandler();

		private BlobTransferManager manager;

		private BlobTransferFileTransferEntry transferEntry;

		private volatile BlobCopyMonitor.State state;

		private Uri sourceUri;

		private ICloudBlob sourceBlob;

		private ICloudBlob destinationBlob;

		private bool moveSource;

		private TransferStatusAndTotalSpeedTracker transferStatusTracker;

		private Timer statusRefreshTimer;

		private object statusRefreshTimerLock = new object();

		private Action<object> startCallback;

		private Action<object, double, double> progressCallback;

		private Action<object, System.Exception> finishCallback;

		private object finishCallbackLock = new object();

		private List<System.Exception> exceptionList = new List<System.Exception>();

		private object exceptionListLock = new object();

		private int activeTasks;

		public long BytesTransferred
		{
			get;
			private set;
		}

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

		public long TotalBytes
		{
			get;
			private set;
		}

		public object UserData
		{
			get;
			private set;
		}

		internal BlobCopyMonitor(BlobTransferManager manager, BlobTransferFileTransferEntry transferEntry, Uri sourceUri, ICloudBlob sourceBlob, ICloudBlob destinationBlob, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, System.Exception> finishCallback, object userData)
		{
			string str;
			if (manager == null || destinationBlob == null || transferEntry == null)
			{
				if (manager == null)
				{
					str = "manager";
				}
				else
				{
					str = (destinationBlob == null ? "destinationBlob" : "transferEntry");
				}
				throw new ArgumentNullException(str);
			}
			if (string.IsNullOrEmpty(transferEntry.CopyId))
			{
				throw new ArgumentException(Resources.TransferEntryCopyIdCannotBeNullOrEmptyException, "transferEntry");
			}
			if (null == sourceUri && sourceBlob == null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothNullException = Resources.ProvideExactlyOneParameterBothNullException;
				object[] objArray = new object[] { "sourceUri", "sourceBlob" };
				throw new ArgumentException(string.Format(invariantCulture, provideExactlyOneParameterBothNullException, objArray));
			}
			if (null != sourceUri && sourceBlob != null)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothProvidedException = Resources.ProvideExactlyOneParameterBothProvidedException;
				object[] objArray1 = new object[] { "sourceUri", "sourceBlob" };
				throw new ArgumentException(string.Format(cultureInfo, provideExactlyOneParameterBothProvidedException, objArray1));
			}
			if (moveSource)
			{
				if (sourceBlob == null)
				{
					throw new ArgumentNullException("sourceBlob", Resources.CannotMoveSourceIfSourceBlobIsNullException);
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
			this.sourceUri = sourceUri ?? sourceBlob.Uri;
			this.sourceBlob = sourceBlob;
			this.destinationBlob = destinationBlob;
			this.moveSource = moveSource;
			this.startCallback = startCallback;
			this.progressCallback = progressCallback;
			this.finishCallback = finishCallback;
			this.UserData = userData;
			this.IsFinished = false;
			this.TotalBytes = (long)-1;
			this.BytesTransferred = (long)-1;
			this.SetInitialStatus();
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
			if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
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

		private void DeleteSourceCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.sourceBlob.EndDelete(asyncResult);
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
				this.DisposeStatusRefreshTimer();
			}
		}

		private void DisposeStatusRefreshTimer()
		{
			if (this.statusRefreshTimer != null)
			{
				lock (this.statusRefreshTimerLock)
				{
					if (this.statusRefreshTimer != null)
					{
						this.statusRefreshTimer.Dispose();
						this.statusRefreshTimer = null;
					}
				}
			}
		}

		private void FetchDestinationAttributesCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.destinationBlob.EndFetchAttributes(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.HandleFetchDestinationAttributesResult(exception, asyncState);
				return;
			}
			this.HandleFetchDestinationAttributesResult(null, asyncState);
		}

		~BlobCopyMonitor()
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
				if (!BlobExtensions.Equals(this.sourceBlob, rootBlob))
				{
					this.sourceBlob = rootBlob;
				}
			}
			else if (this.sourceBlob.SnapshotTime.HasValue)
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
					DeleteSnapshotsOption deleteSnapshotsOption = (this.sourceBlob.SnapshotTime.HasValue ? DeleteSnapshotsOption.None : DeleteSnapshotsOption.IncludeSnapshots);
					try
					{
						this.cancellationHandler.RegisterCancellableAsyncOper(() => this.sourceBlob.BeginDelete(deleteSnapshotsOption, null, blobRequestOptions, operationContext1, new AsyncCallback(this.DeleteSourceCallback), callbackState));
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

		private Action<Action<ITransferController>> GetFetchDestinationAttributesAction()
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
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.destinationBlob.BeginFetchAttributes(null, blobRequestOptions, operationContext1, new AsyncCallback(this.FetchDestinationAttributesCallback), callbackState));
				}
				catch (System.Exception exception1)
				{
					this.HandleFetchDestinationAttributesResult(exception1, callbackState);
				}
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
				case (BlobCopyMonitor.State)BlobCopyMonitor.State.FetchDestinationAttributes:
				{
					return this.GetFetchDestinationAttributesAction();
				}
				case (BlobCopyMonitor.State)BlobCopyMonitor.State.DeleteSource:
				{
					return this.GetDeleteSourceAction();
				}
				case (BlobCopyMonitor.State)BlobCopyMonitor.State.Finished:
				{
					throw new InvalidOperationException();
				}
				case (BlobCopyMonitor.State)BlobCopyMonitor.State.Error:
				{
					throw new InvalidOperationException();
				}
			}
			throw new InvalidOperationException();
		}

		private void HandleFetchDestinationAttributesResult(System.Exception e, CallbackState callbackState)
		{
			bool flag = true;
			if (e != null)
			{
				StorageException storageException = e as StorageException;
				if (storageException == null || storageException.RequestInformation == null || storageException.RequestInformation.HttpStatusCode != 404)
				{
					this.SetErrorState(e, callbackState);
					return;
				}
				flag = false;
			}
			if (flag)
			{
				if (this.destinationBlob.CopyState == null)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string failedToRetrieveCopyStateForBlobToMonitorException = Resources.FailedToRetrieveCopyStateForBlobToMonitorException;
					object[] str = new object[] { this.destinationBlob.Uri.ToString() };
					string str1 = string.Format(invariantCulture, failedToRetrieveCopyStateForBlobToMonitorException, str);
					this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.FailToRetrieveCopyStateForBlobToMonitor, str1), callbackState);
					return;
				}
				if (!this.transferEntry.CopyId.Equals(this.destinationBlob.CopyState.CopyId))
				{
					this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.MismatchCopyId, Resources.MismatchFoundBetweenLocalAndServerCopyIdsException), callbackState);
					return;
				}
				CopyStatus status = this.destinationBlob.CopyState.Status;
				if (CopyStatus.Success != status)
				{
                    if (CopyStatus.Pending != status)
					{
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						string failedToCopyBlobException = Resources.FailedToCopyBlobException;
						object[] objArray = new object[] { this.sourceUri.ToString(), this.destinationBlob.Uri.ToString(), status.ToString() };
						string str2 = string.Format(cultureInfo, failedToCopyBlobException, objArray);
						this.SetErrorState(new BlobTransferException(BlobTransferErrorCode.CopyFromBlobToBlobFailed, str2), callbackState);
					}
					else
					{
						try
						{
							this.UpdateTransferProgress();
						}
						catch (BlobTransferCallbackException blobTransferCallbackException)
						{
							this.SetErrorState(blobTransferCallbackException, callbackState);
							return;
						}
						this.statusRefreshTimer.Change(TimeSpan.FromMilliseconds(100), new TimeSpan((long)-1));
						callbackState.CallFinish(this);
						return;
					}
					return;
				}
				else
				{
					this.DisposeStatusRefreshTimer();
					this.ChangeStatus();
				}
			}
			callbackState.CallFinish(this);
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

		private void SetErrorState(System.Exception ex, CallbackState callbackState)
		{
			lock (this.exceptionListLock)
			{
				this.exceptionList.Add(ex);
			}
			this.state = BlobCopyMonitor.State.Error;
			this.HasWork = false;
			this.IsFinished = true;
			if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
			{
				this.DisposeStatusRefreshTimer();
			}
			this.FinishCallbackHandler(ex);
			callbackState.CallFinish(this);
		}

		private void SetFinished()
		{
			this.state = BlobCopyMonitor.State.Finished;
			this.HasWork = false;
			this.IsFinished = true;
			this.FinishCallbackHandler(null);
		}

		private void SetHasWorkAfterStatusChanged()
		{
			if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
			{
				this.statusRefreshTimer = new Timer((object timerState) => this.HasWork = true);
				this.state = BlobCopyMonitor.State.FetchDestinationAttributes;
			}
			else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
			{
				this.state = BlobCopyMonitor.State.DeleteSource;
			}
			this.HasWork = true;
		}

		private void SetInitialStatus()
		{
			switch (this.transferEntry.Status)
			{
				case BlobTransferEntryStatus.Monitor:
				case BlobTransferEntryStatus.RemoveSource:
				{
					this.SetHasWorkAfterStatusChanged();
					return;
				}
				default:
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string invalidInitialEntryStatusForControllerException = Resources.InvalidInitialEntryStatusForControllerException;
					object[] status = new object[] { this.transferEntry.Status, this.GetType().Name };
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
			if (this.state == BlobCopyMonitor.State.Error)
			{
				throw this.Exception;
			}
		}

		private void UpdateTransferProgress()
		{
			if (this.destinationBlob.CopyState != null && this.destinationBlob.CopyState.TotalBytes.HasValue)
			{
				if (this.transferStatusTracker == null)
				{
					long? totalBytes = this.destinationBlob.CopyState.TotalBytes;
					this.TotalBytes = totalBytes.Value;
					this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(this.TotalBytes, this.manager.TransferOptions.Concurrency, new Action<double, double>(this.ProgressCallbackHandler), this.manager.GlobalCopySpeedTracker);
				}
				long? bytesCopied = this.destinationBlob.CopyState.BytesCopied;
				this.BytesTransferred = bytesCopied.Value;
				this.transferStatusTracker.UpdateStatus(this.BytesTransferred);
			}
		}

		private enum State
		{
			FetchDestinationAttributes,
			DeleteSource,
			Finished,
			Error
		}
	}
}