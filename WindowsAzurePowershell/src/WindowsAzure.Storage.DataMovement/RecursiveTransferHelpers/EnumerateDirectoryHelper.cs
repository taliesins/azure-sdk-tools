using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal static class EnumerateDirectoryHelper
	{
		private static string AppendDirectorySeparator(string dir)
		{
			char chr = dir[dir.Length - 1];
			if (Path.DirectorySeparatorChar != chr && Path.AltDirectorySeparatorChar != chr)
			{
				dir = string.Concat(dir, Path.DirectorySeparatorChar);
			}
			return dir;
		}

		private static void CheckPathDiscoveryPermission(string dir)
		{
			string str = string.Concat(EnumerateDirectoryHelper.AppendDirectorySeparator(dir), '.');
			(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, str)).Demand();
		}

		private static void CheckSearchPattern(string searchPattern)
		{
			while (true)
			{
				int num = searchPattern.IndexOf("..", StringComparison.Ordinal);
				if (-1 == num)
				{
					return;
				}
				num = num + 2;
				if (searchPattern.Length == num || searchPattern[num] == Path.DirectorySeparatorChar || searchPattern[num] == Path.AltDirectorySeparatorChar)
				{
					break;
				}
				searchPattern = searchPattern.Substring(num);
			}
			throw new ArgumentException("Search pattern cannot contain \"..\" to move up directoriesand can be contained only internally in file/directory names, as in \"a..b\"");
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption, CancellationTokenSource cancellationTokenSource)
		{
			IEnumerable<string> strs;
			CancellationChecker cancellationChecker = new CancellationChecker();
			CancellationToken token = cancellationTokenSource.Token;
			CancellationTokenRegistration cancellationTokenRegistration = token.Register(new Action(cancellationChecker.Cancel));
			try
			{
				if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
				{
					throw new ArgumentOutOfRangeException("searchOption");
				}
				searchPattern = searchPattern.TrimEnd(new char[0]);
				if (searchPattern.Length != 0)
				{
					if ("." == searchPattern)
					{
						searchPattern = "*";
					}
					cancellationChecker.CheckCancellation();
					EnumerateDirectoryHelper.CheckSearchPattern(searchPattern);
					cancellationChecker.CheckCancellation();
					string fullPath = Path.GetFullPath(path);
					EnumerateDirectoryHelper.CheckPathDiscoveryPermission(fullPath);
					string directoryName = Path.GetDirectoryName(searchPattern);
					if (!string.IsNullOrEmpty(directoryName))
					{
						EnumerateDirectoryHelper.CheckPathDiscoveryPermission(Path.Combine(fullPath, directoryName));
					}
					string str = Path.Combine(fullPath, searchPattern);
					char chr = str[str.Length - 1];
					if (Path.DirectorySeparatorChar == chr || Path.AltDirectorySeparatorChar == chr || Path.VolumeSeparatorChar == chr)
					{
						str = string.Concat(str, '*');
					}
					string str1 = EnumerateDirectoryHelper.AppendDirectorySeparator(Path.GetDirectoryName(str));
					string str2 = str.Substring(str1.Length);
					if (!Directory.Exists(str1))
					{
						throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'.", str1));
					}
					cancellationChecker.CheckCancellation();
					strs = EnumerateDirectoryHelper.InternalEnumerateFiles(str1, str2, searchOption, cancellationChecker);
				}
				else
				{
					strs = new List<string>();
				}
			}
			finally
			{
				((IDisposable)cancellationTokenRegistration).Dispose();
			}
			return strs;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
		private static extern bool FindClose(SafeHandle findFileHandle);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern EnumerateDirectoryHelper.SafeFindHandle FindFirstFile(string fileName, out EnumerateDirectoryHelper.WIN32_FIND_DATA findFileData);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern bool FindNextFile(SafeHandle findFileHandle, out EnumerateDirectoryHelper.WIN32_FIND_DATA findFileData);

		private static IEnumerable<string> InternalEnumerateFiles(string directoryName, string filePattern, SearchOption searchOption, CancellationChecker cancellationChecker)
		{
			EnumerateDirectoryHelper.WIN32_FIND_DATA wIN32FINDDATum;
			Queue<string> strs = new Queue<string>();
			strs.Enqueue(directoryName);
			while (strs.Count > 0)
			{
				string str = EnumerateDirectoryHelper.AppendDirectorySeparator(strs.Dequeue());
				cancellationChecker.CheckCancellation();
				try
				{
					EnumerateDirectoryHelper.CheckPathDiscoveryPermission(str);
				}
				catch (SecurityException securityException)
				{
					continue;
				}
				using (EnumerateDirectoryHelper.SafeFindHandle safeFindHandle = EnumerateDirectoryHelper.FindFirstFile(string.Concat(str, filePattern), out wIN32FINDDATum))
				{
					if (safeFindHandle.IsInvalid)
					{
						goto Label0;
					}
					while (true)
					{
						cancellationChecker.CheckCancellation();
						if (FileAttributes.Directory != (wIN32FINDDATum.FileAttributes & FileAttributes.Directory))
						{
							yield return Path.Combine(str, wIN32FINDDATum.FileName);
						}
						if (!EnumerateDirectoryHelper.FindNextFile(safeFindHandle, out wIN32FINDDATum))
						{
							break;
						}
					}
				}
			Label0:
				if (SearchOption.AllDirectories != searchOption)
				{
					continue;
				}
				using (EnumerateDirectoryHelper.SafeFindHandle safeFindHandle1 = EnumerateDirectoryHelper.FindFirstFile(string.Concat(str, '*'), out wIN32FINDDATum))
				{
					if (!safeFindHandle1.IsInvalid)
					{
						do
						{
							cancellationChecker.CheckCancellation();
							if (FileAttributes.Directory != (wIN32FINDDATum.FileAttributes & FileAttributes.Directory) || wIN32FINDDATum.FileName.Equals(".") || wIN32FINDDATum.FileName.Equals("..") || FileAttributes.ReparsePoint == (wIN32FINDDATum.FileAttributes & FileAttributes.ReparsePoint))
							{
								continue;
							}
							strs.Enqueue(Path.Combine(str, wIN32FINDDATum.FileName));
						}
						while (EnumerateDirectoryHelper.FindNextFile(safeFindHandle1, out wIN32FINDDATum));
					}
				}
			}
		}

		private sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			[SecurityCritical]
			internal SafeFindHandle() : base(true)
			{
			}

			protected override void Dispose(bool disposing)
			{
				if (!this.IsInvalid && !base.IsClosed)
				{
					EnumerateDirectoryHelper.FindClose(this);
				}
				base.Dispose(disposing);
			}

			protected override bool ReleaseHandle()
			{
				if (!this.IsInvalid && !base.IsClosed)
				{
					return EnumerateDirectoryHelper.FindClose(this);
				}
				if (this.IsInvalid)
				{
					return true;
				}
				return base.IsClosed;
			}
		}

		[BestFitMapping(false)]
		private struct WIN32_FIND_DATA
		{
			public FileAttributes FileAttributes;

			public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;

			public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;

			public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;

			public uint FileSizeHigh;

			public uint FileSizeLow;

			public int Reserved0;

			public int Reserved1;

			public string FileName;

			public string AlternateFileName;
		}
	}
}