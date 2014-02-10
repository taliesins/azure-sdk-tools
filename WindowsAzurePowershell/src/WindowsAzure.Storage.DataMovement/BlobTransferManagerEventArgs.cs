using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferManagerEventArgs : EventArgs
	{
		public double GlobalSpeed
		{
			get;
			private set;
		}

		internal BlobTransferManagerEventArgs(double globalSpeed)
		{
			if (globalSpeed < 0)
			{
				throw new ArgumentOutOfRangeException("globalSpeed");
			}
			this.GlobalSpeed = globalSpeed;
		}
	}
}