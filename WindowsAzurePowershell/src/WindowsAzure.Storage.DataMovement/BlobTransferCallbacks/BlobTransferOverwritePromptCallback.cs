using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferCallbacks
{
	public delegate bool BlobTransferOverwritePromptCallback(string destinationPath);
}