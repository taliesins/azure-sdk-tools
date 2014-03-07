using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	internal static class BlobTransfer_BlobRequestOptionsFactory
	{
		private static TimeSpan defaultMaximumExecutionTime;

		private static int defaultRetryCountXMsError;

		private static int defaultRetryCountOtherError;

		private static TimeSpan retryPoliciesDefaultBackoff;

		private static IRetryPolicy defaultRetryPolicy;

		public static BlobRequestOptions ClearPagesRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions CreateContainerRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions CreatePageBlobRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions DeleteRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions DownloadBlockListRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions FetchAttributesRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions GetBlobReferenceFromServerRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions GetPageRangesRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions ListBlobsRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions OpenReadRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions PutBlockListRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions PutBlockRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions SetMetadataRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions StartCopyFromBlobRequestOptions
		{
			get;
			private set;
		}

		public static BlobRequestOptions WritePagesRequestOptions
		{
			get;
			private set;
		}

		static BlobTransfer_BlobRequestOptionsFactory()
		{
			BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime = TimeSpan.FromSeconds(900);
			BlobTransfer_BlobRequestOptionsFactory.defaultRetryCountXMsError = 10;
			BlobTransfer_BlobRequestOptionsFactory.defaultRetryCountOtherError = 3;
			BlobTransfer_BlobRequestOptionsFactory.retryPoliciesDefaultBackoff = TimeSpan.FromSeconds(3);
			BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy = new BlobTransfer_BlobRequestOptionsFactory.BlobTransferRetryPolicy(BlobTransfer_BlobRequestOptionsFactory.retryPoliciesDefaultBackoff, BlobTransfer_BlobRequestOptionsFactory.defaultRetryCountXMsError, BlobTransfer_BlobRequestOptionsFactory.defaultRetryCountOtherError);
			BlobRequestOptions blobRequestOption = new BlobRequestOptions();
			blobRequestOption.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(15));
			BlobTransfer_BlobRequestOptionsFactory.CreateContainerRequestOptions = blobRequestOption;
			BlobRequestOptions blobRequestOption1 = new BlobRequestOptions();
			blobRequestOption1.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption1.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption1.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(60));
			BlobTransfer_BlobRequestOptionsFactory.ListBlobsRequestOptions = blobRequestOption1;
			BlobRequestOptions blobRequestOption2 = new BlobRequestOptions();
			blobRequestOption2.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption2.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption2.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(15));
			BlobTransfer_BlobRequestOptionsFactory.CreatePageBlobRequestOptions = blobRequestOption2;
			BlobRequestOptions blobRequestOption3 = new BlobRequestOptions();
			blobRequestOption3.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption3.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption3.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(15));
			BlobTransfer_BlobRequestOptionsFactory.DeleteRequestOptions = blobRequestOption3;
			BlobRequestOptions blobRequestOption4 = new BlobRequestOptions();
			blobRequestOption4.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption4.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption4.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(15));
			BlobTransfer_BlobRequestOptionsFactory.FetchAttributesRequestOptions = blobRequestOption4;
			BlobRequestOptions blobRequestOption5 = new BlobRequestOptions();
			blobRequestOption5.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption5.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption5.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(60));
			BlobTransfer_BlobRequestOptionsFactory.GetPageRangesRequestOptions = blobRequestOption5;
			BlobRequestOptions blobRequestOption6 = new BlobRequestOptions();
			blobRequestOption6.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption6.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption6.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(300));
			blobRequestOption6.UseTransactionalMD5=new bool?(true);
			BlobTransfer_BlobRequestOptionsFactory.OpenReadRequestOptions = blobRequestOption6;
			BlobRequestOptions blobRequestOption7 = new BlobRequestOptions();
			blobRequestOption7.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption7.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption7.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(300));
			blobRequestOption7.UseTransactionalMD5=new bool?(true);
			BlobTransfer_BlobRequestOptionsFactory.PutBlockRequestOptions = blobRequestOption7;
			BlobRequestOptions blobRequestOption8 = new BlobRequestOptions();
			blobRequestOption8.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption8.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption8.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(60));
			BlobTransfer_BlobRequestOptionsFactory.PutBlockListRequestOptions = blobRequestOption8;
			BlobRequestOptions blobRequestOption9 = new BlobRequestOptions();
			blobRequestOption9.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption9.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption9.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(60));
			BlobTransfer_BlobRequestOptionsFactory.DownloadBlockListRequestOptions = blobRequestOption9;
			BlobRequestOptions blobRequestOption10 = new BlobRequestOptions();
			blobRequestOption10.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption10.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption10.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(15));
			BlobTransfer_BlobRequestOptionsFactory.SetMetadataRequestOptions = blobRequestOption10;
			BlobRequestOptions blobRequestOption11 = new BlobRequestOptions();
			blobRequestOption11.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption11.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption11.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(300));
			blobRequestOption11.UseTransactionalMD5=new bool?(true);
			BlobTransfer_BlobRequestOptionsFactory.WritePagesRequestOptions = blobRequestOption11;
			BlobRequestOptions blobRequestOption12 = new BlobRequestOptions();
			blobRequestOption12.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption12.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption12.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(300));
			BlobTransfer_BlobRequestOptionsFactory.ClearPagesRequestOptions = blobRequestOption12;
			BlobRequestOptions blobRequestOption13 = new BlobRequestOptions();
			blobRequestOption13.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption13.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption13.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(60));
			BlobTransfer_BlobRequestOptionsFactory.GetBlobReferenceFromServerRequestOptions = blobRequestOption13;
			BlobRequestOptions blobRequestOption14 = new BlobRequestOptions();
			blobRequestOption14.MaximumExecutionTime=new TimeSpan?(BlobTransfer_BlobRequestOptionsFactory.defaultMaximumExecutionTime);
			blobRequestOption14.RetryPolicy=BlobTransfer_BlobRequestOptionsFactory.defaultRetryPolicy;
			blobRequestOption14.ServerTimeout=new TimeSpan?(TimeSpan.FromSeconds(120));
			BlobTransfer_BlobRequestOptionsFactory.StartCopyFromBlobRequestOptions = blobRequestOption14;
		}

		private class BlobTransferRetryPolicy : IRetryPolicy
		{
			private const string XMsPrefix = "x-ms";

			private int maxAttemptsOtherError;

			private ExponentialRetry retryPolicy;

			private bool gotXMsError;

			public BlobTransferRetryPolicy(TimeSpan deltaBackoff, int maxAttemptsXMsError, int maxAttemptsOtherError)
			{
				this.retryPolicy = new ExponentialRetry(deltaBackoff, maxAttemptsXMsError);
				this.maxAttemptsOtherError = maxAttemptsOtherError;
			}

			private BlobTransferRetryPolicy(ExponentialRetry retryPolicy, int maxAttemptsInOtherError)
			{
				this.retryPolicy = retryPolicy;
				this.maxAttemptsOtherError = maxAttemptsInOtherError;
			}

			public IRetryPolicy CreateInstance()
			{
				return new BlobTransfer_BlobRequestOptionsFactory.BlobTransferRetryPolicy(this.retryPolicy.CreateInstance() as ExponentialRetry, this.maxAttemptsOtherError);
			}

			public bool ShouldRetry(int currentRetryCount, int statusCode, Exception lastException, out TimeSpan retryInterval, OperationContext operationContext)
			{
				retryInterval = new TimeSpan();
				if (!this.retryPolicy.ShouldRetry(currentRetryCount, statusCode, lastException, out retryInterval, operationContext))
				{
					return false;
				}
				if (this.gotXMsError)
				{
					return true;
				}
				StorageException storageException = lastException as StorageException;
				if (storageException != null)
				{
					WebException innerException = storageException.InnerException as WebException;
					if (innerException != null)
					{
						HttpWebResponse response = innerException.Response as HttpWebResponse;
						if (response != null && response.Headers != null && response.Headers.AllKeys != null)
						{
							for (int i = 0; i < (int)response.Headers.AllKeys.Length; i++)
							{
								if (response.Headers.AllKeys[i].StartsWith("x-ms"))
								{
									this.gotXMsError = true;
									return true;
								}
							}
						}
					}
				}
				if (currentRetryCount < this.maxAttemptsOtherError)
				{
					return true;
				}
				return false;
			}
		}
	}
}