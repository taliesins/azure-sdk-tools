using Microsoft.WindowsAzure.Storage.DataMovement;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class ReadDataState : TransferDataState
	{
		public Microsoft.WindowsAzure.Storage.DataMovement.MemoryManager MemoryManager
		{
			get;
			set;
		}

		public System.IO.MemoryStream MemoryStream
		{
			get;
			set;
		}

		public ReadDataState()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.MemoryStream != null)
				{
					this.MemoryStream.Dispose();
					this.MemoryStream = null;
				}
				if (this.MemoryManager != null)
				{
					this.MemoryManager.ReleaseBuffer(base.MemoryBuffer);
					this.MemoryManager = null;
				}
			}
		}
	}
}