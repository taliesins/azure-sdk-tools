using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class ErrorFileEntry : FileEntry
	{
		public readonly System.Exception Exception;

		public ErrorFileEntry(System.Exception ex) : base(null, null, null)
		{
			this.Exception = ex;
		}
	}
}