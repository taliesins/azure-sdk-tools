using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using Microsoft.WindowsAzure.Storage.DataMovement.Extensions;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal class BlobStartCopyController : ITransferController
	{
		private AsyncCallCancellationHandler cancellationHandler = new AsyncCallCancellationHandler();

		private BlobTransferManager manager;

		private BlobTransferFileTransferEntry transferEntry;

		private volatile BlobStartCopyController.State state;

		private Uri sourceUri;

		private ICloudBlob sourceBlob;

		private ICloudBlob destinationBlob;

		private CloudBlobContainer destinationContainer;

		private string destinationBlobName;

		private bool monitorProcess;

		private bool moveSource;

		private Action<object> startCallback;

		private Action<object, double, double> progressCallback;

		private Action<object, string, System.Exception> finishCallback;

		private object finishCallbackLock = new object();

		private List<System.Exception> exceptionList = new List<System.Exception>();

		private object exceptionListLock = new object();

		private bool gotDestinationBlob;

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
				return true;
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

		internal BlobStartCopyController(BlobTransferManager manager, BlobTransferFileTransferEntry transferEntry, Uri sourceUri, ICloudBlob sourceBlob, CloudBlobContainer destinationContainer, string destinationBlobName, bool monitorProcess, bool moveSource, Action<object> startCallback, Action<object, double, double> progressCallback, Action<object, string, System.Exception> finishCallback, object userData)
		{
			if (manager == null || transferEntry == null)
			{
				throw new ArgumentNullException((manager == null ? "manager" : "transferEntry"));
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
			if (string.IsNullOrEmpty(destinationBlobName) || destinationContainer == null)
			{
				throw new ArgumentNullException((destinationBlobName == null ? "destinationBlobName" : "destinationContainer"));
			}
			if (moveSource)
			{
				if (sourceBlob == null)
				{
					throw new ArgumentNullException("sourceBlob", Resources.CannotMoveSourceIfSourceBlobIsNullException);
				}
				if (!monitorProcess)
				{
					throw new ArgumentException(Resources.CannotMoveSourceIfMonitoringIsTurnedOffException, "moveSource");
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
			this.destinationBlobName = destinationBlobName;
			this.destinationContainer = destinationContainer;
			this.monitorProcess = monitorProcess;
			this.moveSource = moveSource;
			this.startCallback = startCallback;
			this.progressCallback = progressCallback;
			this.finishCallback = finishCallback;
			this.UserData = userData;
			this.IsFinished = false;
			this.SetInitialStatus();
		}

		private bool CallbackExceptionHandler(Action callbackAction, CallbackState callbackState)
		{
			bool flag;
			try
			{
				callbackAction();
				return true;
			}
			catch (System.Exception exception)
			{
				this.SetErrorState(new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, exception), callbackState);
				flag = false;
			}
			return flag;
		}

		public void CancelWork()
		{
			this.cancellationHandler.Cancel();
		}

		private void ChangeStatus()
		{
			if (BlobTransferEntryStatus.Transfer != this.transferEntry.Status)
			{
				if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
				{
					this.SetFinishedStartCopy();
				}
				return;
			}
			if (!this.monitorProcess)
			{
				this.transferEntry.Status = BlobTransferEntryStatus.Finished;
				this.SetFinished();
				return;
			}
			this.transferEntry.Status = BlobTransferEntryStatus.Monitor;
			this.SetHasWorkAfterStatusChanged(false);
		}

		private void FetchSourceAttributesCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.sourceBlob.EndFetchAttributes(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.HandleFetchSourceAttributesException(exception, asyncState);
				return;
			}
			if (string.IsNullOrEmpty(this.transferEntry.ETag))
			{
				this.transferEntry.ETag = this.sourceBlob.Properties.ETag;
			}
			else if (!this.transferEntry.ETag.Equals(this.sourceBlob.Properties.ETag, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!this.RetransferModifiedCallbackHandler(this.sourceBlob.Name, asyncState))
				{
					return;
				}
				this.transferEntry.ETag = this.sourceBlob.Properties.ETag;
				this.transferEntry.CopyId = null;
				this.transferEntry.Status = BlobTransferEntryStatus.Transfer;
			}
			if (this.sourceBlob.Properties.BlobType == null)
			{
				this.SetErrorState(new InvalidOperationException(Resources.FailedToGetBlobTypeException), asyncState);
				return;
			}
            if (BlobType.BlockBlob != this.sourceBlob.Properties.BlobType && BlobType.PageBlob != this.sourceBlob.Properties.BlobType)
			{
				throw new InvalidOperationException(Resources.OnlySupportTwoBlobTypesException);
			}
			if (!this.ProgressCallbackHandler(asyncState))
			{
				return;
			}
			this.state = BlobStartCopyController.State.GetDestinationBlob;
			this.HasWork = true;
			asyncState.CallFinish(this);
		}

		private void FinishCallbackHandler(string copyId, System.Exception ex)
		{
			try
			{
				if (this.finishCallback != null)
				{
					lock (this.finishCallbackLock)
					{
						if (this.finishCallback != null)
						{
							this.finishCallback(this.UserData, copyId, ex);
							this.finishCallback = null;
						}
					}
				}
			}
			catch (System.Exception exception)
			{
			}
		}

		private void GetDestinationBlobCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.destinationBlob = this.destinationContainer.EndGetBlobReferenceFromServer(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.HandleGetDestinationBlobResult(exception, asyncState);
				return;
			}
			this.HandleGetDestinationBlobResult(null, asyncState);
		}

		private Action<Action<ITransferController>> GetFetchSourceAttributesAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				if (!this.StartCallbackHandler(callbackState))
				{
					return;
				}
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.FetchAttributes);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.sourceBlob.BeginFetchAttributes(null, blobRequestOptions, operationContext1, new AsyncCallback(this.FetchSourceAttributesCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.HandleFetchSourceAttributesException(exception, callbackState);
				}
			};
		}

		private Action<Action<ITransferController>> GetGetDestinationBlobAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				if (!this.StartCallbackHandler(callbackState))
				{
					return;
				}
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.GetBlobReferenceFromServer);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => this.destinationContainer.BeginGetBlobReferenceFromServer(this.destinationBlobName, null, blobRequestOptions, operationContext1, new AsyncCallback(this.GetDestinationBlobCallback), callbackState));
				}
				catch (System.Exception exception)
				{
					this.HandleGetDestinationBlobResult(exception, callbackState);
				}
			};
		}

		private Action<Action<ITransferController>> GetQueueMonitorAction()
		{
			this.HasWork = false;
			if (this.gotDestinationBlob)
			{
				return (Action<ITransferController> finishDelegate) => {
					Action<object, System.Exception> action = null;
					if (this.finishCallback != null)
					{
						action = (object userData, System.Exception ex) => this.finishCallback(userData, null, ex);
					}
					try
					{
						this.manager.QueueBlobCopyMonitor(this.transferEntry, (this.sourceBlob == null ? this.sourceUri : null), this.sourceBlob, this.destinationBlob, this.moveSource, this.startCallback, this.progressCallback, action, this.UserData);
					}
					catch (OperationCanceledException operationCanceledException)
					{
						this.SetErrorState(operationCanceledException, new CallbackState()
						{
							FinishDelegate = finishDelegate
						});
						return;
					}
					this.ChangeStatus();
					finishDelegate(this);
				};
			}
			this.state = BlobStartCopyController.State.GetDestinationBlob;
			return this.GetGetDestinationBlobAction();
		}

		private Action<Action<ITransferController>> GetStartCopyAction()
		{
			this.HasWork = false;
			return (Action<ITransferController> finishDelegate) => {
				BlobRequestOptions blobRequestOptions = this.manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.StartCopyFromBlob);
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				CallbackState callbackState = new CallbackState()
				{
					FinishDelegate = finishDelegate
				};
				try
				{
					this.cancellationHandler.RegisterCancellableAsyncOper(() => {
						if (this.sourceBlob == null)
						{
							return this.destinationBlob.BeginStartCopyFromBlob(this.sourceUri, null, null, blobRequestOptions, operationContext1, new AsyncCallback(this.StartCopyCallback), callbackState);
						}
						AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.sourceBlob.Properties.ETag);
						if (BlobType.PageBlob == this.sourceBlob.Properties.BlobType)
						{
							return (this.destinationBlob as CloudPageBlob).BeginStartCopyFromBlob(this.sourceBlob.AppendSAS() as CloudPageBlob, accessCondition, null, blobRequestOptions, operationContext1, new AsyncCallback(this.StartCopyCallback), callbackState);
						}
						if (BlobType.BlockBlob != this.sourceBlob.Properties.BlobType)
						{
							return null;
						}
						return (this.destinationBlob as CloudBlockBlob).BeginStartCopyFromBlob(this.sourceBlob.AppendSAS() as CloudBlockBlob, accessCondition, null, blobRequestOptions, operationContext1, new AsyncCallback(this.StartCopyCallback), callbackState);
					});
				}
				catch (System.Exception exception)
				{
					this.HandleStartCopyResult(exception, callbackState);
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
				case (BlobStartCopyController.State)BlobStartCopyController.State.FetchSourceAttributes:
				{
					return this.GetFetchSourceAttributesAction();
				}
				case (BlobStartCopyController.State)BlobStartCopyController.State.GetDestinationBlob:
				{
					return this.GetGetDestinationBlobAction();
				}
				case (BlobStartCopyController.State)BlobStartCopyController.State.StartCopy:
				{
					return this.GetStartCopyAction();
				}
				case (BlobStartCopyController.State)BlobStartCopyController.State.QueueMonitor:
				{
					return this.GetQueueMonitorAction();
				}
				case (BlobStartCopyController.State)BlobStartCopyController.State.Finished:
				{
					throw new InvalidOperationException();
				}
				case (BlobStartCopyController.State)BlobStartCopyController.State.Error:
				{
					throw new InvalidOperationException();
				}
			}
			throw new InvalidOperationException();
		}

		private void HandleFetchSourceAttributesException(System.Exception e, CallbackState callbackState)
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

		private void HandleGetDestinationBlobResult(System.Exception e, CallbackState callbackState)
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
			if (!flag)
			{
				if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
				{
					this.transferEntry.Status = BlobTransferEntryStatus.Transfer;
					this.transferEntry.CopyId = null;
				}
				if (this.sourceBlob == null || BlobType.PageBlob != this.sourceBlob.Properties.BlobType)
				{
					this.destinationBlob = this.destinationContainer.GetBlockBlobReference(this.destinationBlobName);
				}
				else
				{
					this.destinationBlob = this.destinationContainer.GetPageBlobReference(this.destinationBlobName);
				}
			}
			else
			{
				if (this.sourceBlob != null && this.sourceBlob.Properties.BlobType != this.destinationBlob.Properties.BlobType)
				{
					this.SetErrorState(new InvalidOperationException((BlobType.PageBlob == this.sourceBlob.Properties.BlobType ? Resources.CannotOverwriteBlockBlobWithPageBlobException : Resources.CannotOverwritePageBlobWithBlockBlobException)), callbackState);
					return;
				}
				if (BlobExtensions.Equals(this.sourceBlob, this.destinationBlob))
				{
					this.SetErrorState(new InvalidOperationException(Resources.SourceAndDestinationLocationCannotBeEqualException), callbackState);
					return;
				}
				if (this.manager.TransferOptions.OverwritePromptCallback != null && !this.OverwritePromptCallbackHandler(this.destinationBlob.Uri.ToString(), callbackState))
				{
					return;
				}
				if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status && string.IsNullOrEmpty(this.transferEntry.CopyId))
				{
					this.SetErrorState(new InvalidOperationException(Resources.RestartableInfoCorruptedException), callbackState);
					return;
				}
			}
			this.gotDestinationBlob = true;
			if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status || BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
			{
				this.state = BlobStartCopyController.State.QueueMonitor;
			}
			else if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
			{
				this.state = BlobStartCopyController.State.StartCopy;
			}
			if (!this.ProgressCallbackHandler(callbackState))
			{
				return;
			}
			this.HasWork = true;
			callbackState.CallFinish(this);
		}

		private void HandleStartCopyResult(System.Exception e, CallbackState callbackState)
		{
			string str;
			DateTimeOffset dateTimeOffset;
			DateTimeOffset? snapshotTime;
			if (e != null)
			{
				StorageException storageException = e as StorageException;
				if (storageException == null || !("PendingCopyOperation" == storageException.RequestInformation.ExtendedErrorInformation.ErrorCode))
				{
					this.SetErrorState(e, callbackState);
					return;
				}
				BlobRequestOptions blobRequestOption = new BlobRequestOptions();
				blobRequestOption.RetryPolicy=new NoRetry();
				BlobRequestOptions blobRequestOption1 = blobRequestOption;
				OperationContext operationContext = new OperationContext();
				operationContext.ClientRequestID=this.manager.TransferOptions.GetClientRequestId();
				OperationContext operationContext1 = operationContext;
				try
				{
					this.cancellationHandler.CheckCancellation();
					this.destinationBlob.FetchAttributes(null, blobRequestOption1, operationContext1);
				}
				catch (System.Exception exception)
				{
					this.SetErrorState(e, callbackState);
					return;
				}
				Uri source = this.destinationBlob.CopyState.Source;
				string components = source.GetComponents(UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
				string components1 = this.sourceUri.GetComponents(UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
				DateTimeOffset? nullable = null;
				if (this.sourceBlob == null)
				{
					snapshotTime = null;
				}
				else
				{
					snapshotTime = this.sourceBlob.SnapshotTime;
				}
				DateTimeOffset? nullable1 = snapshotTime;
				if (BlobStartCopyController.ParseQueryString(source.Query).TryGetValue("snapshot", out str) && !string.IsNullOrEmpty(str) && DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dateTimeOffset))
				{
					nullable = new DateTimeOffset?(dateTimeOffset);
				}
				if (components.Equals(components1) && nullable.Equals(nullable1))
				{
					goto Label1;
				}
				this.SetErrorState(e, callbackState);
				return;
			}
			this.ChangeStatus();
			if (!this.ProgressCallbackHandler(callbackState))
			{
				return;
			}
			callbackState.CallFinish(this);
			return;
		Label1:
			if (string.IsNullOrEmpty(this.transferEntry.CopyId))
			{
				this.transferEntry.CopyId = this.destinationBlob.CopyState.CopyId;
				this.ChangeStatus();
				if (!this.ProgressCallbackHandler(callbackState))
				{
					return;
				}
				callbackState.CallFinish(this);
				return;
			}
			else
			{
				this.ChangeStatus();
				if (!this.ProgressCallbackHandler(callbackState))
				{
					return;
				}
				callbackState.CallFinish(this);
				return;
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

		private static Dictionary<string, string> ParseQueryString(string query)
		{
			Dictionary<string, string> strs = new Dictionary<string, string>();
			if (query == null || query.Length == 0)
			{
				return strs;
			}
			if (query.StartsWith("?"))
			{
				query = query.Substring(1);
			}
			string[] strArrays = new string[] { "&" };
			string[] strArrays1 = query.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				int num = str.IndexOf("=");
				if (num >= 0)
				{
					string str1 = str.Substring(0, num);
					string str2 = str.Substring(num + 1);
					strs.Add(Uri.UnescapeDataString(str1), Uri.UnescapeDataString(str2));
				}
				else
				{
					strs.Add(Uri.UnescapeDataString(str), null);
				}
			}
			return strs;
		}

		public int PostWork()
		{
			return Interlocked.Decrement(ref this.activeTasks);
		}

		public void PreWork()
		{
			Interlocked.Increment(ref this.activeTasks);
		}

		private bool ProgressCallbackHandler(CallbackState callbackState)
		{
			return this.CallbackExceptionHandler(() => {
				if (this.progressCallback != null)
				{
					this.progressCallback(this.UserData, 0, 0);
				}
			}, callbackState);
		}

		private bool PromptCallBackExceptionHandler(BlobStartCopyController.PromptCallbackAction promptcallbackAction, CallbackState callbackState)
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

		private void SetErrorState(System.Exception ex, CallbackState callbackState)
		{
			lock (this.exceptionListLock)
			{
				this.exceptionList.Add(ex);
			}
			this.state = BlobStartCopyController.State.Error;
			this.HasWork = false;
			this.IsFinished = true;
			this.FinishCallbackHandler(null, ex);
			callbackState.CallFinish(this);
		}

		private void SetFinished()
		{
			this.SetFinishedStartCopy();
			this.FinishCallbackHandler(this.transferEntry.CopyId, null);
		}

		private void SetFinishedStartCopy()
		{
			this.state = BlobStartCopyController.State.Finished;
			this.HasWork = false;
			this.IsFinished = true;
		}

		private void SetHasWorkAfterStatusChanged(bool initSet)
		{
			if (!initSet && BlobTransferEntryStatus.Transfer != this.transferEntry.Status)
			{
				if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status || BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
				{
					this.state = BlobStartCopyController.State.QueueMonitor;
				}
			}
			else if (this.sourceBlob != null)
			{
				this.state = BlobStartCopyController.State.FetchSourceAttributes;
			}
			else
			{
				this.state = BlobStartCopyController.State.GetDestinationBlob;
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
					this.SetHasWorkAfterStatusChanged(true);
					return;
				}
				case BlobTransferEntryStatus.Transfer:
				case BlobTransferEntryStatus.Monitor:
				case BlobTransferEntryStatus.RemoveSource:
				{
					this.SetHasWorkAfterStatusChanged(true);
					return;
				}
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

		private bool StartCallbackHandler(CallbackState callbackState)
		{
			return this.CallbackExceptionHandler(() => {
				if (this.startCallback != null)
				{
					this.startCallback(this.UserData);
					this.startCallback = null;
				}
			}, callbackState);
		}

		private void StartCopyCallback(IAsyncResult asyncResult)
		{
			this.cancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);
			CallbackState asyncState = asyncResult.AsyncState as CallbackState;
			try
			{
				this.transferEntry.CopyId = this.destinationBlob.EndStartCopyFromBlob(asyncResult);
			}
			catch (System.Exception exception)
			{
				this.HandleStartCopyResult(exception, asyncState);
				return;
			}
			this.HandleStartCopyResult(null, asyncState);
		}

		private void ThrowOnErrorState()
		{
			if (this.state == BlobStartCopyController.State.Error)
			{
				throw this.Exception;
			}
		}

		private delegate bool PromptCallbackAction();

		private enum State
		{
			FetchSourceAttributes,
			GetDestinationBlob,
			StartCopy,
			QueueMonitor,
			Finished,
			Error
		}
	}
}