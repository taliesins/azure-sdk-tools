using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferCallbacks
{
	public delegate bool BlobTransferBeforeQueueCallback(string sourcePath, string destinationPath);
}