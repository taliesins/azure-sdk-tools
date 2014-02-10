using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers;
using Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal class BlobTransferRecursiveTransferItem : ITransferController
	{
		private volatile BlobTransferRecursiveTransferItem.State state;

		private ILocation sourceLocation;

		private ILocation destinationLocation;

		private BlobTransferRecursiveTransferOptions options;

		private BlobTransferManager manager;

		private Action<object> startCallback;

		private Action<object, System.Exception> finishCallback;

		private object finishCallbackLock = new object();

		private Func<EntryData, Action<object>> getStartFileCallback;

		private Func<EntryData, Action<object, double, double>> getProgressFileCallback;

		private Func<EntryData, BlobTransferFileTransferEntry, Action<object, System.Exception>> getFinishFileCallback;

		private List<System.Exception> exceptionList = new List<System.Exception>();

		private object exceptionListLock = new object();

		private CancellationChecker cancellationChecker = new CancellationChecker();

		private bool hasWork;

		private bool isFinished;

		private int activeTasks;

		public bool CanAddController
		{
			get
			{
				return true;
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
				return this.hasWork;
			}
		}

		public bool IsFinished
		{
			get
			{
				return this.isFinished;
			}
		}

		public object UserData
		{
			get;
			private set;
		}

		internal BlobTransferRecursiveTransferItem(BlobTransferManager manager, string sourceLocation, string destinationLocation, BlobTransferRecursiveTransferOptions options, Action<object> startCallback, Action<object, System.Exception> finishCallback, Action<object, EntryData> startFileCallback, Action<object, EntryData, double, double> progressFileCallback, Action<object, EntryData, System.Exception> finishFileCallback, object userData)
		{
			BlobTransferRecursiveTransferItem blobTransferRecursiveTransferItem = this;
			if (manager == null || options == null)
			{
				throw new ArgumentNullException((manager == null ? "manager" : "options"));
			}
			this.manager = manager;
			try
			{
				this.sourceLocation = Location.CreateLocation(sourceLocation, options.SourceKey, options.SourceSAS, this.manager.TransferOptions, true);
			}
			catch (System.Exception exception2)
			{
				System.Exception exception1 = exception2;
				throw new BlobTransferException(BlobTransferErrorCode.InvalidSourceLocation, exception1.Message, exception1);
			}
			try
			{
				this.destinationLocation = Location.CreateLocation(destinationLocation, options.DestinationKey, options.DestinationSAS, this.manager.TransferOptions, false);
			}
			catch (System.Exception exception4)
			{
				System.Exception exception3 = exception4;
				throw new BlobTransferException(BlobTransferErrorCode.InvalidDestinationLocation, exception3.Message, exception3);
			}
			if (this.sourceLocation is FileSystemLocation && this.destinationLocation is FileSystemLocation)
			{
				throw new BlobTransferException(BlobTransferErrorCode.LocalToLocalTransfersUnsupported, Resources.LocalToLocalTransferUnsupportedException);
			}
			if (Location.Equals(this.sourceLocation, this.destinationLocation))
			{
				throw new BlobTransferException(BlobTransferErrorCode.SameSourceAndDestination, Resources.SourceAndDestinationLocationCannotBeEqualException);
			}
			this.options = options;
			this.startCallback = startCallback;
			this.finishCallback = finishCallback;
			this.getStartFileCallback = (EntryData entryData) => (object data) => {
				if (startFileCallback != null)
				{
					startFileCallback(blobTransferRecursiveTransferItem.UserData, entryData);
				}
			};
			this.getProgressFileCallback = (EntryData entryData) => (object data, double speed, double progress) => {
				if (progressFileCallback != null)
				{
					progressFileCallback(blobTransferRecursiveTransferItem.UserData, entryData, speed, progress);
				}
			};
			this.getFinishFileCallback = (EntryData entryData, BlobTransferFileTransferEntry transferEntry) => (object data, System.Exception exception) => {
				if (exception == null && transferEntry != null)
				{
					transferEntry.Status = BlobTransferEntryStatus.Finished;
				}
				if (finishFileCallback != null)
				{
					finishFileCallback(blobTransferRecursiveTransferItem.UserData, entryData, exception);
				}
			};
			this.state = BlobTransferRecursiveTransferItem.State.EnumerateFiles;
			this.hasWork = true;
			this.isFinished = false;
			this.UserData = userData;
		}

		private bool BeforeQueueCallbackHandler(string sourcePath, string destinationPath, EntryData entryData)
		{
			bool beforeQueueCallback;
			if (this.options.BeforeQueueCallback == null)
			{
				return true;
			}
			try
			{
				beforeQueueCallback = this.options.BeforeQueueCallback(sourcePath, destinationPath);
			}
			catch (System.Exception exception)
			{
				this.OutputFailedFileEntry(entryData, new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, exception));
				beforeQueueCallback = false;
			}
			return beforeQueueCallback;
		}

		public void CancelWork()
		{
			this.cancellationChecker.Cancel();
		}

		private bool CheckFileAttributes(FileEntry entry)
		{
			string absolutePath = (this.sourceLocation as FileSystemLocation).GetAbsolutePath(entry.RelativePath);
			FileAttributes attributes = File.GetAttributes(absolutePath);
			if (this.options.OnlyFilesWithArchiveBit && FileAttributes.Archive != (attributes & FileAttributes.Archive))
			{
				return false;
			}
			if ((int)this.options.IncludedAttributes != 0 && (int)(attributes & this.options.IncludedAttributes) == 0)
			{
				return false;
			}
			if ((int)this.options.ExcludedAttributes != 0 && (int)(attributes & this.options.ExcludedAttributes) != 0)
			{
				return false;
			}
			return true;
		}

		private static void CreateTargetDirectory(string location)
		{
			string directoryName = Path.GetDirectoryName(location);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}

		private void FinishCallbackHandler(System.Exception ex)
		{
			if (this.finishCallback != null)
			{
				try
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
				catch (System.Exception)
				{
				}
			}
		}

		private Action<Action<ITransferController>> GetEnumerateFilesAction()
		{
			this.hasWork = false;
			return (Action<ITransferController> finishDelegate) => ThreadPool.QueueUserWorkItem((object param0) => this.PerformTransfer(new CallbackState()
			{
				FinishDelegate = finishDelegate
			}));
		}

		public Action<Action<ITransferController>> GetWork()
		{
			this.ThrowOnErrorState();
			if (!this.hasWork)
			{
				return null;
			}
			switch (this.state)
			{
				case (BlobTransferRecursiveTransferItem.State)BlobTransferRecursiveTransferItem.State.EnumerateFiles:
				{
					return this.GetEnumerateFilesAction();
				}
				case (BlobTransferRecursiveTransferItem.State)BlobTransferRecursiveTransferItem.State.Finished:
				{
					throw new InvalidOperationException();
				}
				case (BlobTransferRecursiveTransferItem.State)BlobTransferRecursiveTransferItem.State.Error:
				{
					throw new InvalidOperationException();
				}
			}
			throw new InvalidOperationException();
		}

		private void OutputFailedFileEntry(EntryData entryData, System.Exception ex)
		{
			this.getStartFileCallback(entryData)(null);
			this.getFinishFileCallback(entryData, null)(null, ex);
		}

		private void PerformTransfer(CallbackState callbackState)
		{
			KeyValuePair<BlobType, int> keyValuePair;
			bool flag;
			bool flag1;
			DateTimeOffset? lastModified;
			BlobType blobType;
			ICloudBlob blobObject;
			try
			{
				bool flag2 = this.sourceLocation is AzureStorageLocation;
				bool flag3 = this.destinationLocation is AzureStorageLocation;
				bool flag4 = (this.options.ExcludeNewer ? true : this.options.ExcludeOlder);
				FileEntryCache fileEntryCache = new FileEntryCache(this.destinationLocation, flag4, this.manager.CancellationTokenSource);
				if (flag4)
				{
					flag = true;
				}
				else
				{
					flag = (flag2 ? false : this.options.RestartableMode);
				}
				flag4 = flag;
				if (fileEntryCache.Exception != null)
				{
					System.Exception exception = fileEntryCache.Exception;
					if (!(exception is DirectoryNotFoundException))
					{
						StorageException storageException = exception as StorageException;
						if (storageException == null || storageException.RequestInformation == null || storageException.RequestInformation.ExtendedErrorInformation == null || "ContainerNotFound" != storageException.RequestInformation.ExtendedErrorInformation.ErrorCode)
						{
							throw new BlobTransferException(BlobTransferErrorCode.InvalidDestinationLocation, exception.Message, exception);
						}
					}
				}
				bool transferSnapshots = !this.options.TransferSnapshots;
				if (!(this.sourceLocation is FileSystemLocation))
				{
					flag1 = false;
				}
				else
				{
					flag1 = (this.options.OnlyFilesWithArchiveBit || (int)this.options.IncludedAttributes != 0 ? true : 0 != (int)this.options.ExcludedAttributes);
				}
				bool flag5 = flag1;
				BlobTransferFileTransferEntries fileEntries = null;
				if (this.options.FileTransferStatus == null || !this.options.FileTransferStatus.Initialized)
				{
					fileEntries = (this.options.FileTransferStatus == null ? new BlobTransferFileTransferEntries() : this.options.FileTransferStatus.FileEntries);
					IFileNameResolver fileNameResolver = FileNameResolver.GetFileNameResolver(this.sourceLocation, this.destinationLocation);
					FileNameSnapshotAppender fileNameSnapshotAppender = new FileNameSnapshotAppender();
					foreach (FileEntry fileEntry in this.sourceLocation.EnumerateLocation(this.options.FilePatterns, this.options.Recursive, flag4, this.manager.CancellationTokenSource))
					{
						this.cancellationChecker.CheckCancellation();
						if (fileEntry is ErrorFileEntry)
						{
							ErrorFileEntry errorFileEntry = fileEntry as ErrorFileEntry;
							throw new BlobTransferException(BlobTransferErrorCode.InvalidSourceLocation, errorFileEntry.Exception.Message, errorFileEntry.Exception);
						}
						if (fileEntry.SnapshotTime.HasValue && transferSnapshots || flag5 && !this.CheckFileAttributes(fileEntry))
						{
							continue;
						}
						string str = fileNameResolver.ResolveFileName(fileEntry);
						FileEntry fileEntry1 = fileEntryCache.GetFileEntry(str);
						if (fileEntry1 != null)
						{
							if (this.options.ExcludeNewer)
							{
								DateTimeOffset? nullable = fileEntry.LastModified;
								DateTimeOffset? lastModified1 = fileEntry1.LastModified;
								if ((nullable.HasValue & lastModified1.HasValue ? nullable.GetValueOrDefault() > lastModified1.GetValueOrDefault() : false))
								{
									continue;
								}
							}
							if (this.options.ExcludeOlder)
							{
								DateTimeOffset? nullable1 = fileEntry.LastModified;
								DateTimeOffset? lastModified2 = fileEntry1.LastModified;
								if ((nullable1.HasValue & lastModified2.HasValue ? nullable1.GetValueOrDefault() < lastModified2.GetValueOrDefault() : false))
								{
									continue;
								}
							}
						}
						string str1 = fileNameSnapshotAppender.ResolveFileName(fileEntry);
						AzureFileEntry azureFileEntry = fileEntry as AzureFileEntry;
						BlobTransferFileTransferEntries blobTransferFileTransferEntry = fileEntries;
						string str2 = str1;
						string relativePath = fileEntry.RelativePath;
						string str3 = str;
						if (flag4)
						{
							lastModified = fileEntry.LastModified;
						}
						else
						{
							lastModified = null;
						}
						DateTimeOffset? snapshotTime = fileEntry.SnapshotTime;
						if (azureFileEntry == null)
						{
							blobType = 0;
						}
						else
						{
							blobType = azureFileEntry.Blob.Properties.BlobType;
						}
						blobTransferFileTransferEntry[str2] = new BlobTransferFileTransferEntry(relativePath, str3, lastModified, snapshotTime, blobType);
					}
					if (this.options.FileTransferStatus != null)
					{
						this.options.FileTransferStatus.Initialized = true;
					}
				}
				else
				{
					fileEntries = this.options.FileTransferStatus.FileEntries;
				}
				if (flag2 && this.options.MoveFile)
				{
					AzureStorageLocation azureStorageLocation = this.sourceLocation as AzureStorageLocation;
					HashSet<string> strs = new HashSet<string>();
					Dictionary<string, KeyValuePair<BlobType, int>> strs1 = new Dictionary<string, KeyValuePair<BlobType, int>>();
					foreach (BlobTransferFileTransferEntry blobTransferFileTransferEntry1 in 
						from x in fileEntries.Values
						where !x.EntryTransferFinished
						select x)
					{
						this.cancellationChecker.CheckCancellation();
						string sourceRelativePath = blobTransferFileTransferEntry1.SourceRelativePath;
						bool flag6 = strs.Contains(sourceRelativePath);
						if (blobTransferFileTransferEntry1.SourceSnapshotTime.HasValue || flag6)
						{
							if (!strs1.TryGetValue(sourceRelativePath, out keyValuePair))
							{
								strs1.Add(sourceRelativePath, new KeyValuePair<BlobType, int>(blobTransferFileTransferEntry1.SourceBlobType, (flag6 ? 2 : 1)));
							}
							else
							{
								strs1[sourceRelativePath] = new KeyValuePair<BlobType, int>(keyValuePair.Key, keyValuePair.Value + 1);
							}
						}
						if (flag6)
						{
							continue;
						}
						strs.Add(sourceRelativePath);
					}
					Dictionary<string, BlobTransferFileTransferEntry.DeletionBlobSet> strs2 = new Dictionary<string, BlobTransferFileTransferEntry.DeletionBlobSet>();
					foreach (KeyValuePair<string, KeyValuePair<BlobType, int>> keyValuePair1 in strs1)
					{
						this.cancellationChecker.CheckCancellation();
						string key = keyValuePair1.Key;
						string key1 = keyValuePair1.Key;
						KeyValuePair<BlobType, int> value = keyValuePair1.Value;
						ICloudBlob cloudBlob = azureStorageLocation.GetBlobObject(key1, value.Key);
						value = keyValuePair1.Value;
						strs2.Add(key, new BlobTransferFileTransferEntry.DeletionBlobSet(cloudBlob, value.Value));
					}
					foreach (BlobTransferFileTransferEntry item in fileEntries.Values.Where<BlobTransferFileTransferEntry>((BlobTransferFileTransferEntry x) => {
						if (x.EntryTransferFinished)
						{
							return false;
						}
						return strs1.ContainsKey(x.SourceRelativePath);
					}))
					{
						this.cancellationChecker.CheckCancellation();
						item.BlobSet = strs2[item.SourceRelativePath];
					}
				}
				this.StartCallbackHandler();
				bool flag7 = false;
				foreach (KeyValuePair<string, BlobTransferFileTransferEntry> keyValuePair2 in 
					from x in fileEntries
					where !x.Value.EntryTransferFinished
					select x)
				{
					this.cancellationChecker.CheckCancellation();
					BlobTransferFileTransferEntry value1 = keyValuePair2.Value;
					string key2 = keyValuePair2.Key;
					string destinationRelativePath = value1.DestinationRelativePath;
					EntryData entryDatum = new EntryData()
					{
						FileName = key2,
						TransferEntry = keyValuePair2.Value
					};
					EntryData blobObject1 = entryDatum;
					if (flag2)
					{
						blobObject1.SourceBlob = (this.sourceLocation as AzureStorageLocation).GetBlobObject(value1.SourceRelativePath, value1.SourceSnapshotTime, value1.SourceBlobType);
					}
					if (flag3)
					{
						EntryData entryDatum1 = blobObject1;
						if (flag2)
						{
							blobObject = null;
						}
						else
						{
							blobObject = (this.destinationLocation as AzureStorageLocation).GetBlobObject(destinationRelativePath, this.options.UploadBlobType);
						}
						entryDatum1.DestinationBlob = blobObject;
					}
					if (this.options.FakeTransfer)
					{
						this.getStartFileCallback(blobObject1)(null);
					}
					else if (flag3)
					{
						AzureStorageLocation azureStorageLocation1 = this.destinationLocation as AzureStorageLocation;
						if (!azureStorageLocation1.ContainerName.Equals("$root") || -1 == destinationRelativePath.IndexOf('/'))
						{
							if (!flag7)
							{
								if (!azureStorageLocation1.StorageCredential.IsSAS)
								{
									try
									{
										BlobTransferOptions transferOptions = this.manager.TransferOptions;
										BlobRequestOptions blobRequestOptions = transferOptions.GetBlobRequestOptions(BlobRequestOperation.CreateContainer);
										OperationContext operationContext = new OperationContext();
										operationContext.ClientRequestID = transferOptions.GetClientRequestId();
										azureStorageLocation1.BlobContainer.CreateIfNotExists(blobRequestOptions, operationContext);
									}
									catch (StorageException storageException2)
									{
										StorageException storageException1 = storageException2;
										throw new BlobTransferException(BlobTransferErrorCode.InvalidDestinationLocation, storageException1.Message, storageException1);
									}
								}
								flag7 = true;
							}
							if (!flag2)
							{
								string absolutePath = (this.sourceLocation as FileSystemLocation).GetAbsolutePath(value1.SourceRelativePath);
								ICloudBlob destinationBlob = blobObject1.DestinationBlob;
								if (!this.BeforeQueueCallbackHandler(absolutePath, destinationBlob.Uri.AbsoluteUri, blobObject1))
								{
									continue;
								}
								try
								{
									this.manager.QueueUpload(value1, destinationBlob, absolutePath, null, this.options.MoveFile, this.getStartFileCallback(blobObject1), this.getProgressFileCallback(blobObject1), this.getFinishFileCallback(blobObject1, value1), null);
								}
								catch (InvalidOperationException invalidOperationException)
								{
									this.OutputFailedFileEntry(blobObject1, invalidOperationException);
								}
							}
							else
							{
								ICloudBlob sourceBlob = blobObject1.SourceBlob;
								if (!this.BeforeQueueCallbackHandler(sourceBlob.Uri.AbsoluteUri, azureStorageLocation1.GetAbsoluteUri(destinationRelativePath), blobObject1))
								{
									continue;
								}
								this.manager.QueueBlobCopy(value1, null, sourceBlob, azureStorageLocation1.BlobContainer, azureStorageLocation1.GetPathUnderContainer(destinationRelativePath), this.options.MoveFile, this.getStartFileCallback(blobObject1), this.getProgressFileCallback(blobObject1), this.getFinishFileCallback(blobObject1, value1), null);
							}
						}
						else
						{
							this.OutputFailedFileEntry(blobObject1, new BlobTransferException(BlobTransferErrorCode.InvalidDestinationLocation, Resources.SubfoldersNotAllowedUnderRootContainerException));
						}
					}
					else
					{
						ICloudBlob sourceBlob1 = blobObject1.SourceBlob;
						string absolutePath1 = (this.destinationLocation as FileSystemLocation).GetAbsolutePath(destinationRelativePath);
						if (!this.BeforeQueueCallbackHandler(sourceBlob1.Uri.AbsoluteUri, absolutePath1, blobObject1))
						{
							continue;
						}
						if (!flag7)
						{
							try
							{
								BlobTransferRecursiveTransferItem.CreateTargetDirectory((this.destinationLocation as FileSystemLocation).FullPath);
							}
							catch (System.Exception exception2)
							{
								System.Exception exception1 = exception2;
								throw new BlobTransferException(BlobTransferErrorCode.InvalidDestinationLocation, exception1.Message, exception1);
							}
							flag7 = true;
						}
						try
						{
							BlobTransferRecursiveTransferItem.CreateTargetDirectory(absolutePath1);
						}
						catch (System.Exception exception3)
						{
							this.OutputFailedFileEntry(blobObject1, exception3);
							continue;
						}
						this.manager.QueueDownload(value1, sourceBlob1, absolutePath1, null, this.options.DownloadCheckMd5, this.options.MoveFile, this.getStartFileCallback(blobObject1), this.getProgressFileCallback(blobObject1), this.getFinishFileCallback(blobObject1, value1), null);
					}
				}
			}
			catch (System.Exception exception4)
			{
				this.SetErrorState(exception4, callbackState);
				return;
			}
			this.hasWork = false;
			this.isFinished = true;
			this.state = BlobTransferRecursiveTransferItem.State.Finished;
			this.FinishCallbackHandler(null);
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

		private void SetErrorState(System.Exception ex, CallbackState callbackState)
		{
			lock (this.exceptionListLock)
			{
				this.exceptionList.Add(ex);
			}
			this.state = BlobTransferRecursiveTransferItem.State.Error;
			this.hasWork = false;
			this.isFinished = true;
			this.FinishCallbackHandler(ex);
			callbackState.CallFinish(this);
		}

		private void StartCallbackHandler()
		{
			if (this.startCallback != null)
			{
				try
				{
					this.startCallback(this.UserData);
					this.startCallback = null;
				}
				catch (System.Exception exception)
				{
					throw new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, exception);
				}
			}
		}

		private void ThrowOnErrorState()
		{
			if (this.state == BlobTransferRecursiveTransferItem.State.Error)
			{
				throw this.Exception;
			}
		}

		private enum State
		{
			EnumerateFiles,
			Finished,
			Error
		}
	}
}