using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class FileSystemToAzureFileNameResolver : IFileNameResolver
	{
		public FileSystemToAzureFileNameResolver()
		{
		}

		public string ResolveFileName(FileEntry sourceEntry)
		{
			return sourceEntry.RelativePath.Replace('\\', '/');
		}
	}
}