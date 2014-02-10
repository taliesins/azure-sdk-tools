using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers
{
	internal class CancellationChecker
	{
		private volatile bool cancelled;

		public CancellationChecker()
		{
		}

		public void Cancel()
		{
			this.cancelled = true;
		}

		public void CheckCancellation()
		{
			if (this.cancelled)
			{
				throw new OperationCanceledException(Resources.BlobTransferCancelledException);
			}
		}
	}
}