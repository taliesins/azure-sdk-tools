using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class FileSystemLocation : ILocation
	{
		private const int MaxPathLength = 247;

		public string FullPath
		{
			get;
			private set;
		}

		public FileSystemLocation(string location)
		{
			string fullPath = Path.GetFullPath(location);
			if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				fullPath = string.Concat(fullPath, Path.DirectorySeparatorChar);
			}
			this.FullPath = fullPath;
		}

		public IEnumerable<FileEntry> EnumerateLocation(IEnumerable<string> filePatterns, bool recursive, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource)
		{
			DateTimeOffset? nullable;
			ErrorFileEntry errorFileEntry;
			CancellationChecker cancellationChecker = new CancellationChecker();
			CancellationToken token = cancellationTokenSource.Token;
			CancellationTokenRegistration cancellationTokenRegistration = token.Register(new Action(cancellationChecker.Cancel));
			try
			{
				IEnumerable<string> filePatternWithDefault = this.GetFilePatternWithDefault(filePatterns);
				HashSet<string> strs = new HashSet<string>();
				int num = this.GetMaxFileNameLength();
				using (IEnumerator<string> enumerator = filePatternWithDefault.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						if (current.Length > num)
						{
							continue;
						}
						SearchOption searchOption = (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
						IEnumerable<string> strs1 = null;
						errorFileEntry = null;
						cancellationChecker.CheckCancellation();
						try
						{
							strs1 = EnumerateDirectoryHelper.EnumerateFiles(this.FullPath, current, searchOption, cancellationTokenSource);
						}
						catch (Exception exception)
						{
							errorFileEntry = new ErrorFileEntry(exception);
						}
						if (errorFileEntry != null)
						{
							goto Label0;
						}
						if (strs1 == null)
						{
							continue;
						}
						using (IEnumerator<string> enumerator1 = strs1.GetEnumerator())
						{
							while (enumerator1.MoveNext())
							{
								string str = enumerator1.Current;
								cancellationChecker.CheckCancellation();
								string str1 = str;
								if (str1.StartsWith(this.FullPath, StringComparison.OrdinalIgnoreCase))
								{
									str1 = str1.Remove(0, this.FullPath.Length);
								}
								if (strs.Contains(str1))
								{
									continue;
								}
								strs.Add(str1);
								DateTime? nullable1 = null;
								if (getLastModifiedTime)
								{
									nullable1 = new DateTime?(File.GetLastWriteTimeUtc(str));
								}
								string str2 = str1;
								DateTime? nullable2 = nullable1;
								if (nullable2.HasValue)
								{
									nullable = new DateTimeOffset?(nullable2.GetValueOrDefault());
								}
								else
								{
									nullable = null;
								}
								yield return new FileEntry(str2, nullable);
							}
						}
					}
					goto Label1;
				Label0:
					yield return errorFileEntry;
				}
			}
			finally
			{
				((IDisposable)cancellationTokenRegistration).Dispose();
			}
		Label1:
			yield break;
		}

		public string GetAbsolutePath(string fileName)
		{
			return Path.Combine(this.FullPath, fileName);
		}

		public IEnumerable<string> GetFilePatternWithDefault(IEnumerable<string> filePatterns)
		{
			if (filePatterns != null && filePatterns.Any<string>())
			{
				return filePatterns;
			}
			return new List<string>()
			{
				"*"
			};
		}

		public int GetMaxFileNameLength()
		{
			return 247 - this.FullPath.Length;
		}
	}
}