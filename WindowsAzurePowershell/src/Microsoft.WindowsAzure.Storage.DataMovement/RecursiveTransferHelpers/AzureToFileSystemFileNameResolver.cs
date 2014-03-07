using Microsoft.WindowsAzure.Storage.DataMovement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class AzureToFileSystemFileNameResolver : IFileNameResolver
	{
		private readonly static string[] reservedBaseFileNames;

		private readonly static string[] reservedFileNames;

		private static char[] invalidFileNameChars;

		private static char[] invalidPathChars;

		private static Regex translateSlashesRegex;

		private Dictionary<string, string> resolvedFilesCache = new Dictionary<string, string>();

		private Func<int> getMaxFileNameLength;

		static AzureToFileSystemFileNameResolver()
		{
			string[] strArrays = new string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
			AzureToFileSystemFileNameResolver.reservedBaseFileNames = strArrays;
			AzureToFileSystemFileNameResolver.reservedFileNames = new string[] { "CLOCK$" };
			AzureToFileSystemFileNameResolver.invalidFileNameChars = Path.GetInvalidFileNameChars();
			AzureToFileSystemFileNameResolver.invalidPathChars = AzureToFileSystemFileNameResolver.GetInvalidPathChars();
			AzureToFileSystemFileNameResolver.translateSlashesRegex = new Regex("(^/)|(?<=/)/|(/$)", RegexOptions.Compiled);
		}

		public AzureToFileSystemFileNameResolver(Func<int> getMaxFileNameLength)
		{
			this.getMaxFileNameLength = getMaxFileNameLength;
		}

		private static string EscapeInvalidCharacters(string fileName, params char[] invalidChars)
		{
			if (invalidChars != null)
			{
				char[] chrArray = invalidChars;
				for (int i = 0; i < (int)chrArray.Length; i++)
				{
					char chr = chrArray[i];
					fileName = fileName.Replace(chr.ToString(), string.Format("%{0:X2}", (int)chr));
				}
			}
			return fileName;
		}

		private static char[] GetInvalidPathChars()
		{
			HashSet<char> chrs = new HashSet<char>(Path.GetInvalidPathChars());
			char[] chrArray = AzureToFileSystemFileNameResolver.invalidFileNameChars;
			for (int i = 0; i < (int)chrArray.Length; i++)
			{
				char chr = chrArray[i];
				if (92 != chr && 47 != chr && !chrs.Contains(chr))
				{
					chrs.Add(chr);
				}
			}
			AzureToFileSystemFileNameResolver.invalidPathChars = new char[chrs.Count];
			chrs.CopyTo(AzureToFileSystemFileNameResolver.invalidPathChars);
			return AzureToFileSystemFileNameResolver.invalidPathChars;
		}

		private static bool IsReservedFileName(string fileName)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			string str = Path.GetFileName(fileName);
			if (Array.Exists<string>(AzureToFileSystemFileNameResolver.reservedBaseFileNames, (string s) => fileNameWithoutExtension.Equals(s, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}
			if (Array.Exists<string>(AzureToFileSystemFileNameResolver.reservedFileNames, (string s) => str.Equals(s, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return true;
			}
			bool flag = true;
			int num = 0;
			while (num < fileName.Length)
			{
				if (fileName[num] == '.' || char.IsWhiteSpace(fileName[num]))
				{
					num++;
				}
				else
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
			return false;
		}

		public string ResolveFileName(FileEntry sourceEntry)
		{
			string fileName;
			string empty;
			string str = AzureToFileSystemFileNameResolver.TranslateSlashes(sourceEntry.RelativePath).TrimEnd(new char[] { ' ' });
			int num = str.LastIndexOf('\\');
			if (-1 != num)
			{
				empty = str.Substring(0, num + 1);
				fileName = str.Substring(num + 1);
			}
			else
			{
				empty = string.Empty;
				fileName = str;
			}
			if (!string.IsNullOrEmpty(empty))
			{
				empty = AzureToFileSystemFileNameResolver.EscapeInvalidCharacters(empty, AzureToFileSystemFileNameResolver.invalidPathChars);
			}
			if (!string.IsNullOrEmpty(fileName))
			{
				fileName = AzureToFileSystemFileNameResolver.EscapeInvalidCharacters(fileName, AzureToFileSystemFileNameResolver.invalidFileNameChars);
			}
			fileName = Utils.AppendSnapShotToFileName(fileName, sourceEntry.SnapshotTime);
			str = Path.Combine(empty, fileName);
			str = this.ResolveFileNameConflict(str);
			this.resolvedFilesCache.Add(str.ToLowerInvariant(), str);
			return str;
		}

		private string ResolveFileNameConflict(string baseFileName)
		{
			int num = this.getMaxFileNameLength();
			Func<string, bool> func = (string fileName) => {
				if (this.resolvedFilesCache.ContainsKey(fileName.ToLowerInvariant()) || AzureToFileSystemFileNameResolver.IsReservedFileName(fileName))
				{
					return true;
				}
				return fileName.Length > num;
			};
			Func<string, string, int, string> func1 = (string fileName, string extension, int count) => {
				string str = string.Format(" ({0})", count);
				int length = fileName.Length + str.Length + extension.Length - num;
				if (length > 0)
				{
					fileName = fileName.Remove(fileName.Length - length);
				}
				return string.Format("{0}{1}{2}", fileName, str, extension);
			};
			return FileNameResolver.ResolveFileNameConflict(baseFileName, func, func1);
		}

		private static string TranslateSlashes(string source)
		{
			string str = AzureToFileSystemFileNameResolver.translateSlashesRegex.Replace(source, string.Format("%{0:X2}", 47));
			return str.Replace('/', '\\');
		}
	}
}