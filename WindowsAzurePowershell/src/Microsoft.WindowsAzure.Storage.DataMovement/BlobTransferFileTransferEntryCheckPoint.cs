using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferFileTransferEntryCheckPoint
	{
		public const int MaxUploadWindowLength = 128;

		public long EntryTransferOffset
		{
			get;
			internal set;
		}

		public List<long> UploadWindow
		{
			get;
			internal set;
		}

		public BlobTransferFileTransferEntryCheckPoint(long entryTransferOffset, List<long> uploadWindow)
		{
			this.EntryTransferOffset = entryTransferOffset;
			if (uploadWindow == null)
			{
				this.UploadWindow = null;
				return;
			}
			this.UploadWindow = new List<long>(uploadWindow);
		}

		internal BlobTransferFileTransferEntryCheckPoint(BlobTransferFileTransferEntryCheckPoint checkPoint) : this(checkPoint.EntryTransferOffset, checkPoint.UploadWindow)
		{
		}

		internal BlobTransferFileTransferEntryCheckPoint() : this((long)0, null)
		{
		}

		internal void Clear()
		{
			this.EntryTransferOffset = (long)0;
			if (this.UploadWindow != null)
			{
				this.UploadWindow.Clear();
			}
		}
	}
}