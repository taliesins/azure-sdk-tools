using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferFileTransferStatus
	{
		public BlobTransferFileTransferEntries FileEntries
		{
			get;
			set;
		}

		public bool Initialized
		{
			get;
			set;
		}

		public BlobTransferFileTransferStatus()
		{
			this.FileEntries = new BlobTransferFileTransferEntries();
			this.Initialized = false;
		}

		public BlobTransferFileTransferStatus(BlobTransferFileTransferEntries fileEntries, bool initalized)
		{
			this.FileEntries = fileEntries;
			this.Initialized = initalized;
		}
	}
}