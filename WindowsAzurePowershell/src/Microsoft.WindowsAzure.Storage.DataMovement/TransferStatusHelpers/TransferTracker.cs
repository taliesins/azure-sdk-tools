using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal abstract class TransferTracker
	{
		private Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers.TransferSpeedCalculator transferSpeedCalculator;

		protected Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers.TransferSpeedCalculator TransferSpeedCalculator
		{
			get
			{
				return this.transferSpeedCalculator;
			}
		}

		protected TransferTracker(int concurrency)
		{
			this.transferSpeedCalculator = new Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers.TransferSpeedCalculator(concurrency);
		}

		public void AddBytesTransferred(long bytesToIncrease)
		{
			this.TriggerCallback(this.transferSpeedCalculator.AddBytesTransferred(bytesToIncrease));
		}

		protected abstract void TriggerCallback(long bytesTransferred);

		public long UpdateStatus(long total)
		{
			long num = this.transferSpeedCalculator.UpdateStatus(total);
			this.TriggerCallback(total);
			return num;
		}
	}
}