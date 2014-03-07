using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.Extensions
{
	internal static class BlobExtensions
	{
		internal static ICloudBlob AppendSAS(this ICloudBlob blob)
		{
			if (blob == null)
			{
				throw new ArgumentNullException("blob");
			}
			if (blob.ServiceClient.Credentials.IsSAS)
			{
				return blob;
			}
			TimeSpan timeSpan = TimeSpan.FromMinutes((double)Math.Max(10, 10080));
			SharedAccessBlobPolicy sharedAccessBlobPolicy = new SharedAccessBlobPolicy();
			DateTime now = DateTime.Now;
			sharedAccessBlobPolicy.SharedAccessExpiryTime=new DateTimeOffset?(now.Add(timeSpan));
            sharedAccessBlobPolicy.Permissions = SharedAccessBlobPermissions.Read;
			SharedAccessBlobPolicy sharedAccessBlobPolicy1 = sharedAccessBlobPolicy;
            if (BlobType.PageBlob == blob.BlobType)
			{
				CloudPageBlob cloudPageBlob = new CloudPageBlob(blob.Uri, blob.ServiceClient.Credentials);
				string sharedAccessSignature = cloudPageBlob.GetSharedAccessSignature(sharedAccessBlobPolicy1);
				return new CloudPageBlob(blob.Uri, blob.SnapshotTime, new StorageCredentials(sharedAccessSignature));
			}
			if (BlobType.BlockBlob != blob.BlobType)
			{
				throw new InvalidOperationException(Resources.OnlySupportTwoBlobTypesException);
			}
			CloudBlockBlob cloudBlockBlob = new CloudBlockBlob(blob.Uri, blob.ServiceClient.Credentials);
			string str = cloudBlockBlob.GetSharedAccessSignature(sharedAccessBlobPolicy1);
			return new CloudBlockBlob(blob.Uri, blob.SnapshotTime, new StorageCredentials(str));
		}

		internal static bool Equals(ICloudBlob blob, ICloudBlob comparand)
		{
			if (blob == comparand)
			{
				return true;
			}
			if (blob == null || comparand == null)
			{
				return false;
			}
			if (!blob.Uri.Equals(comparand.Uri))
			{
				return false;
			}
			DateTimeOffset? snapshotTime = blob.SnapshotTime;
			return snapshotTime.Equals(comparand.SnapshotTime);
		}
	}
}