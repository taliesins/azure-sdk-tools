using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class TransferSpeedTracker : TransferTracker
	{
		private Action<double> speedCallback;

		public TransferSpeedTracker(Action<double> speedCallback, int concurrency) : base(concurrency)
		{
			this.speedCallback = speedCallback;
		}

		protected override void TriggerCallback(long bytesTransferred)
		{
			if (this.speedCallback != null)
			{
				double num = base.TransferSpeedCalculator.CalculateSpeed(bytesTransferred);
				this.speedCallback(num);
			}
		}
	}
}