using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal abstract class TransferDataState : IDisposable
	{
		public int BytesRead
		{
			get;
			set;
		}

		public Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers.CallbackState CallbackState
		{
			get;
			set;
		}

		public int Length
		{
			get;
			set;
		}

		public byte[] MemoryBuffer
		{
			get;
			set;
		}

		public int SequenceNumber
		{
			get;
			set;
		}

		public long StartOffset
		{
			get;
			set;
		}

		protected TransferDataState()
		{
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected abstract void Dispose(bool disposing);
	}
}