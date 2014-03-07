using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public static class BlobTransferConstants
	{
		public const int MaxBlockSize = 4194304;

		public const int DefaultBlockSize = 4194304;

		internal const string DefaultContainerName = "$root";

		internal const int MinBlockSize = 262144;

		internal const long MaxPageBlobFileSize = 1099511627776L;

		internal const long MaxBlockBlobFileSize = 209715200000L;

		internal const int MaxUploadWindowSize = 128;

		internal const long PageRangesSpanSize = 155189248L;

		internal const long DefaultMemoryCacheSize = 1073741824L;

		internal const double MemoryCacheMultiplier = 0.5;

		internal const long MemoryCacheMaximum = 2147483648L;

		internal const int MemoryManagerCellsMaximum = 8192;

		internal const int BlobCopySASLifeTimeInMinutes = 10080;

		internal const long BlobCopyStatusRefreshWaitTimeInMilliseconds = 100L;
	}
}