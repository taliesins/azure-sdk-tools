using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal interface IFileNameResolver
	{
		string ResolveFileName(FileEntry sourceEntry);
	}
}