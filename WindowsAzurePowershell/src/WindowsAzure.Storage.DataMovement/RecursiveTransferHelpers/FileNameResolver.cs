using System;
using System.IO;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal static class FileNameResolver
	{
		public static IFileNameResolver GetFileNameResolver(ILocation sourceLocation, ILocation destinationLocation)
		{
			bool flag = sourceLocation is AzureStorageLocation;
			if (flag == destinationLocation is AzureStorageLocation)
			{
				if (flag)
				{
					return new FileNameSnapshotAppender();
				}
				return null;
			}
			if (!flag)
			{
				return new FileSystemToAzureFileNameResolver();
			}
			ILocation location = destinationLocation;
			return new AzureToFileSystemFileNameResolver(new Func<int>(location.GetMaxFileNameLength));
		}

		public static string ResolveFileNameConflict(string baseFileName, Func<string, bool> conflict, Func<string, string, int, string> construct)
		{
			if (!conflict(baseFileName))
			{
				return baseFileName;
			}
			string str = Path.ChangeExtension(baseFileName, null);
			string extension = Path.GetExtension(baseFileName);
			string empty = string.Empty;
			int num = 1;
			do
			{
				empty = construct(str, extension, num);
				num++;
			}
			while (conflict(empty));
			return empty;
		}
	}
}