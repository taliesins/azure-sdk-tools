using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferCallbacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferOptions
	{
		private int blockSize;

		private Dictionary<BlobRequestOperation, BlobRequestOptions> allBlobRequestOptions = new Dictionary<BlobRequestOperation, BlobRequestOptions>();

		private string baseClientRequestID;

		private string clientRequestId;

		private int concurrency;

		private long maximumCacheSize;

		public int BlockSize
		{
			get
			{
				return this.blockSize;
			}
			set
			{
				if (262144 > value || value > 4194304)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string blockSizeOutOfRangeException = Resources.BlockSizeOutOfRangeException;
					object[] humanReadableSize = new object[] { Utils.BytesToHumanReadableSize(262144), Utils.BytesToHumanReadableSize(4194304) };
					string str = string.Format(invariantCulture, blockSizeOutOfRangeException, humanReadableSize);
					throw new ArgumentOutOfRangeException("value", (object)value, str);
				}
				this.blockSize = value;
			}
		}

		public int Concurrency
		{
			get
			{
				return this.concurrency;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ConcurrentCountNotPositiveException, new object[0]));
				}
				this.concurrency = value;
			}
		}

		public long MaximumCacheSize
		{
			get
			{
				return this.maximumCacheSize;
			}
			set
			{
				if (value < (long)4194304)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string smallMemoryCacheSizeLimitationException = Resources.SmallMemoryCacheSizeLimitationException;
					object[] humanReadableSize = new object[] { Utils.BytesToHumanReadableSize(4194304) };
					throw new ArgumentException(string.Format(invariantCulture, smallMemoryCacheSizeLimitationException, humanReadableSize));
				}
				this.maximumCacheSize = value;
			}
		}

		public BlobTransferOverwritePromptCallback OverwritePromptCallback
		{
			get;
			set;
		}

		public BlobTransferRetransferModifiedFileCallback RetransferModifiedCallback
		{
			get;
			set;
		}

		public BlobTransferOptions()
		{
			this.Concurrency = Environment.ProcessorCount * 8;
			this.BlockSize = 4194304;
			GlobalMemoryStatusNativeMethods globalMemoryStatusNativeMethod = new GlobalMemoryStatusNativeMethods();
			if ((long)0 != globalMemoryStatusNativeMethod.AvailablePhysicalMemory)
			{
				this.MaximumCacheSize = Math.Min((long)((double)((float)globalMemoryStatusNativeMethod.AvailablePhysicalMemory) * 0.5), (long)int.MaxValue);
			}
			else
			{
				this.MaximumCacheSize = (long)1073741824;
			}
			this.baseClientRequestID = BlobTransferOptions.GetDataMovementClientRequestID();
			this.clientRequestId = this.baseClientRequestID;
		}

		public void AppendToClientRequestId(string postfix)
		{
			this.clientRequestId = string.Concat(this.baseClientRequestID, " ", postfix);
		}

		public BlobRequestOptions GetBlobRequestOptions(BlobRequestOperation operation)
		{
			BlobRequestOptions blobRequestOption;
			if (this.allBlobRequestOptions.TryGetValue(operation, out blobRequestOption))
			{
				return blobRequestOption;
			}
			switch (operation)
			{
				case BlobRequestOperation.CreateContainer:
				{
					return BlobTransfer_BlobRequestOptionsFactory.CreateContainerRequestOptions;
				}
				case BlobRequestOperation.ListBlobs:
				{
					return BlobTransfer_BlobRequestOptionsFactory.ListBlobsRequestOptions;
				}
				case BlobRequestOperation.CreatePageBlob:
				{
					return BlobTransfer_BlobRequestOptionsFactory.CreatePageBlobRequestOptions;
				}
				case BlobRequestOperation.Delete:
				{
					return BlobTransfer_BlobRequestOptionsFactory.DeleteRequestOptions;
				}
				case BlobRequestOperation.GetPageRanges:
				{
					return BlobTransfer_BlobRequestOptionsFactory.GetPageRangesRequestOptions;
				}
				case BlobRequestOperation.OpenRead:
				{
					return BlobTransfer_BlobRequestOptionsFactory.OpenReadRequestOptions;
				}
				case BlobRequestOperation.PutBlock:
				{
					return BlobTransfer_BlobRequestOptionsFactory.PutBlockRequestOptions;
				}
				case BlobRequestOperation.PutBlockList:
				{
					return BlobTransfer_BlobRequestOptionsFactory.PutBlockListRequestOptions;
				}
				case BlobRequestOperation.DownloadBlockList:
				{
					return BlobTransfer_BlobRequestOptionsFactory.DownloadBlockListRequestOptions;
				}
				case BlobRequestOperation.SetMetadata:
				{
					return BlobTransfer_BlobRequestOptionsFactory.SetMetadataRequestOptions;
				}
				case BlobRequestOperation.FetchAttributes:
				{
					return BlobTransfer_BlobRequestOptionsFactory.FetchAttributesRequestOptions;
				}
				case BlobRequestOperation.WritePages:
				{
					return BlobTransfer_BlobRequestOptionsFactory.WritePagesRequestOptions;
				}
				case BlobRequestOperation.ClearPages:
				{
					return BlobTransfer_BlobRequestOptionsFactory.ClearPagesRequestOptions;
				}
				case BlobRequestOperation.GetBlobReferenceFromServer:
				{
					return BlobTransfer_BlobRequestOptionsFactory.GetBlobReferenceFromServerRequestOptions;
				}
				case BlobRequestOperation.StartCopyFromBlob:
				{
					return BlobTransfer_BlobRequestOptionsFactory.StartCopyFromBlobRequestOptions;
				}
			}
			throw new ArgumentOutOfRangeException("operation");
		}

		public string GetClientRequestId()
		{
			return this.clientRequestId;
		}

		private static string GetDataMovementClientRequestID()
		{
			AssemblyName name = Assembly.GetExecutingAssembly().GetName();
			return string.Concat(name.Name, "/", name.Version.ToString());
		}

		public void SetBlobRequestOptions(BlobRequestOperation operation, BlobRequestOptions blobRequestOptions)
		{
			this.allBlobRequestOptions[operation] = blobRequestOptions;
		}
	}
}