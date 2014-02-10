using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class AzureFileEntry : FileEntry
	{
		public ICloudBlob Blob
		{
			get;
			private set;
		}

		public AzureFileEntry(string relativePath, ICloudBlob cloudBlob) : base(relativePath, cloudBlob.Properties.LastModified, cloudBlob.SnapshotTime)
		{
			this.Blob = cloudBlob;
		}
	}
}