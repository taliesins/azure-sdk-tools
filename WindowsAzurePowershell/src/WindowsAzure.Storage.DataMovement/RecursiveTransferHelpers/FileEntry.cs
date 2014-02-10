using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class FileEntry
	{
		public DateTimeOffset? LastModified
		{
			get;
			private set;
		}

		public string RelativePath
		{
			get;
			private set;
		}

		public DateTimeOffset? SnapshotTime
		{
			get;
			private set;
		}

		public FileEntry(FileEntry entry) : this(entry.RelativePath, entry.LastModified, entry.SnapshotTime)
		{
		}

		public FileEntry(string relativePath) : this(relativePath, null, null)
		{
		}

		public FileEntry(string relativePath, DateTimeOffset? lastModified) : this(relativePath, lastModified, null)
		{
		}

		public FileEntry(string relativePath, DateTimeOffset? lastModified, DateTimeOffset? snapshotTime)
		{
			this.RelativePath = relativePath;
			this.LastModified = lastModified;
			this.SnapshotTime = snapshotTime;
		}
	}
}