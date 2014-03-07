using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class TransferStatusAndTotalSpeedTracker
	{
		private TransferStatusTracker statusTracker;

		private TransferSpeedTracker globalSpeedTracker;

		public TransferStatusAndTotalSpeedTracker(long totalSize, int concurrency, Action<double, double> progressCallback, TransferSpeedTracker globalSpeedTracker)
		{
			this.statusTracker = new TransferStatusTracker(totalSize, concurrency, progressCallback);
			this.globalSpeedTracker = globalSpeedTracker;
		}

		public void AddBytesTransferred(long bytesToIncrease)
		{
			if (this.statusTracker != null)
			{
				this.statusTracker.AddBytesTransferred(bytesToIncrease);
			}
			if (this.globalSpeedTracker != null)
			{
				this.globalSpeedTracker.AddBytesTransferred(bytesToIncrease);
			}
		}

		public void UpdateStatus(long total)
		{
			long num = (long)0;
			if (this.statusTracker != null)
			{
				num = this.statusTracker.UpdateStatus(total);
			}
			if (this.globalSpeedTracker != null)
			{
				this.globalSpeedTracker.AddBytesTransferred(total - num);
			}
		}
	}
}