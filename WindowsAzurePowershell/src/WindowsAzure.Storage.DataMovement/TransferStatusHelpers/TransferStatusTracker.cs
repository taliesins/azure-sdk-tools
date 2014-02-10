using System;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class TransferStatusTracker : TransferTracker
	{
		private long totalSize;

		private Action<double, double> progressCallback;

		protected long TotalSize
		{
			get
			{
				return Interlocked.Read(ref this.totalSize);
			}
		}

		public TransferStatusTracker(long totalSize, int concurrency, Action<double, double> progressCallback) : base(concurrency)
		{
			this.totalSize = totalSize;
			this.progressCallback = progressCallback;
		}

		protected override void TriggerCallback(long bytesTransferred)
		{
			if (this.progressCallback != null)
			{
				double num = base.TransferSpeedCalculator.CalculateSpeed(bytesTransferred);
				this.progressCallback(num, ((long)0 == this.totalSize ? 100 : (double)bytesTransferred / (double)this.TotalSize * 100));
			}
		}
	}
}