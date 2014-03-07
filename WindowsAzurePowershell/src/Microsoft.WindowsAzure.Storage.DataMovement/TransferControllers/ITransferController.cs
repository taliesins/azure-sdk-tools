using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.TransferControllers
{
	internal interface ITransferController
	{
		bool CanAddController
		{
			get;
		}

		bool CanAddMonitor
		{
			get;
		}

		System.Exception Exception
		{
			get;
		}

		bool HasWork
		{
			get;
		}

		bool IsFinished
		{
			get;
		}

		void CancelWork();

		Action<Action<ITransferController>> GetWork();

		int PostWork();

		void PreWork();
	}
}