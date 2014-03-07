using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class EntryData
	{
		public ICloudBlob DestinationBlob
		{
			get;
			internal set;
		}

		public string FileName
		{
			get;
			internal set;
		}

		public ICloudBlob SourceBlob
		{
			get;
			internal set;
		}

		public BlobTransferFileTransferEntry TransferEntry
		{
			get;
			internal set;
		}

		public EntryData()
		{
		}
	}
}