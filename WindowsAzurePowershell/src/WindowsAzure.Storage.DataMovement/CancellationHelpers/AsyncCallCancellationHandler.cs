using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers
{
	internal class AsyncCallCancellationHandler
	{
		private ConcurrentDictionary<ICancellableAsyncResult, object> asyncOperations = new ConcurrentDictionary<ICancellableAsyncResult, object>();

		private CancellationChecker cancellationChecker = new CancellationChecker();

		public AsyncCallCancellationHandler()
		{
		}

		public void Cancel()
		{
			this.cancellationChecker.Cancel();
			foreach (KeyValuePair<ICancellableAsyncResult, object> asyncOperation in this.asyncOperations)
			{
				asyncOperation.Key.Cancel();
			}
			this.asyncOperations.Clear();
		}

		public void CheckCancellation()
		{
			this.cancellationChecker.CheckCancellation();
		}

		public void DeregisterCancellableAsyncOper(ICancellableAsyncResult cancellableAsyncResult)
		{
			object obj;
			this.asyncOperations.TryRemove(cancellableAsyncResult, out obj);
		}

		public void RegisterCancellableAsyncOper(AsyncCallCancellationHandler.CancellableAsyncCall asyncCall)
		{
			this.CheckCancellation();
			this.asyncOperations.TryAdd(asyncCall(), null);
		}

		internal delegate ICancellableAsyncResult CancellableAsyncCall();
	}
}