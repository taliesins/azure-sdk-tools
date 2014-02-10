using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class TransferSpeedCalculator
	{
		private int queueSize;

		private long totalBytes;

		private Queue<long> timeQueue;

		private Queue<long> bytesQueue;

		private object lockCalculateSpeed = new object();

		internal long TotalBytes
		{
			get
			{
				return Interlocked.Read(ref this.totalBytes);
			}
		}

		public TransferSpeedCalculator(int concurrency)
		{
			this.queueSize = concurrency + 1;
			this.timeQueue = new Queue<long>(this.queueSize);
			this.bytesQueue = new Queue<long>(this.queueSize);
			this.CalculateSpeed((long)0);
		}

		public long AddBytesTransferred(long bytesTransfered)
		{
			return Interlocked.Add(ref this.totalBytes, bytesTransfered);
		}

		public double CalculateSpeed(long total)
		{
			double num;
			lock (this.lockCalculateSpeed)
			{
				if (!this.bytesQueue.Any<long>() || this.bytesQueue.Last<long>() != total)
				{
					if (this.timeQueue.Count == this.queueSize)
					{
						this.timeQueue.Dequeue();
						this.bytesQueue.Dequeue();
					}
					long ticks = DateTime.Now.Ticks;
					this.timeQueue.Enqueue(ticks);
					this.bytesQueue.Enqueue(total);
				}
				double totalSeconds = 0;
				if (this.timeQueue.Count > 1)
				{
					double num1 = (double)(this.bytesQueue.Last<long>() - this.bytesQueue.First<long>());
					TimeSpan timeSpan = TimeSpan.FromTicks(this.timeQueue.Last<long>() - this.timeQueue.First<long>());
					totalSeconds = num1 / timeSpan.TotalSeconds;
				}
				num = totalSeconds;
			}
			return num;
		}

		public long UpdateStatus(long total)
		{
			return Interlocked.Exchange(ref this.totalBytes, total);
		}
	}
}