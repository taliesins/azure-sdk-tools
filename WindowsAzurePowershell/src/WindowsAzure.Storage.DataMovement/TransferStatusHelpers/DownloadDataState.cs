using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class DownloadDataState : TransferDataState
	{
		public System.IO.MemoryStream MemoryStream
		{
			get;
			set;
		}

		public Microsoft.WindowsAzure.Storage.OperationContext OperationContext
		{
			get;
			set;
		}

		public DownloadDataState()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.MemoryStream != null)
			{
				this.MemoryStream.Dispose();
				this.MemoryStream = null;
			}
		}
	}
}