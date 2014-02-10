using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.Exceptions
{
	public enum BlobTransferErrorCode
	{
		None,
		InvalidSourceLocation,
		InvalidDestinationLocation,
		OpenFileFailed,
		UploadBlobSourceFileSizeTooLarge,
		UploadBlobSourceFileSizeInvalid,
		OperationCanceled,
		LocalToLocalTransfersUnsupported,
		CopyFromBlobToBlobFailed,
		SameSourceAndDestination,
		MismatchCopyId,
		FailToRetrieveCopyStateForBlobToMonitor,
		FailToAllocateMemory,
		FailToGetSourceLastWriteTime
	}
}