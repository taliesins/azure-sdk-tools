using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public enum BlobTransferEntryStatus
	{
		NotStarted,
		Transfer,
		Monitor,
		RemoveSource,
		Finished
	}
}