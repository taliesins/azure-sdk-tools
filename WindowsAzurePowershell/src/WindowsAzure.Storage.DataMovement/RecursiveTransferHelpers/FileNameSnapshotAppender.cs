using Microsoft.WindowsAzure.Storage.DataMovement;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class FileNameSnapshotAppender : IFileNameResolver
	{
		private HashSet<string> fileNameSet = new HashSet<string>();

		public FileNameSnapshotAppender()
		{
		}

		public string ResolveFileName(FileEntry sourceEntry)
		{
			string str = FileNameResolver.ResolveFileNameConflict(Utils.AppendSnapShotToFileName(sourceEntry.RelativePath, sourceEntry.SnapshotTime), new Func<string, bool>(this.fileNameSet.Contains), (string fileName, string extension, int count) => string.Format("{0} ({1}){2}", fileName, count, extension));
			this.fileNameSet.Add(str);
			return str;
		}
	}
}