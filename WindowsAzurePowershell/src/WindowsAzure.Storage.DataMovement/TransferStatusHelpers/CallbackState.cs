using Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferStatusHelpers
{
	internal class CallbackState
	{
		public Action<ITransferController> FinishDelegate
		{
			get;
			set;
		}

		public CallbackState()
		{
		}

		public void CallFinish(ITransferController transferController)
		{
			this.FinishDelegate(transferController);
		}
	}
}