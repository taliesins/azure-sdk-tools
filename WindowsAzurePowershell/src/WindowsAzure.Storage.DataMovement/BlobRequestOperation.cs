using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public enum BlobRequestOperation
	{
		CreateContainer,
		ListBlobs,
		CreatePageBlob,
		Delete,
		GetPageRanges,
		OpenRead,
		PutBlock,
		PutBlockList,
		DownloadBlockList,
		SetMetadata,
		FetchAttributes,
		WritePages,
		ClearPages,
		GetBlobReferenceFromServer,
		StartCopyFromBlob
	}
}