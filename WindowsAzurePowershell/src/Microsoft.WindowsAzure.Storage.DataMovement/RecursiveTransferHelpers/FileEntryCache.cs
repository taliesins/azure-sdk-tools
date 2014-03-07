using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class FileEntryCache
	{
		private Dictionary<string, FileEntry> fileEntryCache;

		private bool azureSource;

		public System.Exception Exception
		{
			get;
			private set;
		}

		public FileEntryCache(ILocation cachedLocation, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource)
		{
			this.fileEntryCache = new Dictionary<string, FileEntry>();
			this.azureSource = cachedLocation is AzureStorageLocation;
			foreach (FileEntry fileEntry in cachedLocation.EnumerateLocation(null, true, getLastModifiedTime, cancellationTokenSource))
			{
				ErrorFileEntry errorFileEntry = fileEntry as ErrorFileEntry;
				if (errorFileEntry != null)
				{
					this.Exception = errorFileEntry.Exception;
					this.fileEntryCache = null;
					break;
				}
				else
				{
					if (fileEntry.SnapshotTime.HasValue)
					{
						continue;
					}
					string str = this.UniformFileName(fileEntry.RelativePath);
					this.fileEntryCache[str] = fileEntry;
				}
			}
		}

		public FileEntry GetFileEntry(string fileName)
		{
			FileEntry fileEntry;
			if (this.Exception != null)
			{
				return null;
			}
			fileName = this.UniformFileName(fileName);
			if (this.fileEntryCache.TryGetValue(fileName, out fileEntry))
			{
				return fileEntry;
			}
			return null;
		}

		private string UniformFileName(string fileName)
		{
			if (this.azureSource)
			{
				return fileName;
			}
			return fileName.ToLowerInvariant();
		}
	}
}