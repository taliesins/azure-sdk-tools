using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.DataMovement.CancellationHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal class AzureStorageLocation : ILocation
	{
		private const string LogsContainerName = "$logs";

		private const int ListBlobsSegmentSize = 250;

		private const int MaxBlobNameLength = 1024;

		private readonly bool EndPointPathStyle;

		private BlobTransferOptions transferOptions;

		public string AccountName
		{
			get;
			private set;
		}

		public string BaseAddress
		{
			get;
			private set;
		}

		public CloudBlobContainer BlobContainer
		{
			get;
			private set;
		}

		public string ContainerName
		{
			get;
			private set;
		}

		public string Folder
		{
			get;
			private set;
		}

		public StorageCredentials StorageCredential
		{
			get;
			private set;
		}

		public AzureStorageLocation(string location, string storageKey, string containerSAS, BlobTransferOptions transferOptions, bool isSourceLocation)
		{
			IPAddress pAddress;
			if (!isSourceLocation && string.IsNullOrEmpty(storageKey) && string.IsNullOrEmpty(containerSAS))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string provideExactlyOneParameterBothNullException = Resources.ProvideExactlyOneParameterBothNullException;
				object[] objArray = new object[] { "storageKey", "containerSAS" };
				throw new ArgumentException(string.Format(invariantCulture, provideExactlyOneParameterBothNullException, objArray));
			}
			if (!string.IsNullOrEmpty(storageKey) && !string.IsNullOrEmpty(containerSAS))
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string provideAtMostOneParameterBothProvidedException = Resources.ProvideAtMostOneParameterBothProvidedException;
				object[] objArray1 = new object[] { "storageKey", "containerSAS" };
				throw new ArgumentException(string.Format(cultureInfo, provideAtMostOneParameterBothProvidedException, objArray1));
			}
			Uri uri = new Uri(location);
			this.EndPointPathStyle = IPAddress.TryParse(uri.Host, out pAddress);
			string empty = string.Empty;
			if (!this.EndPointPathStyle)
			{
				string host = uri.Host;
				char[] chrArray = new char[] { '.' };
				this.AccountName = host.Split(chrArray, 2)[0];
				this.BaseAddress = uri.GetLeftPart(UriPartial.Authority);
				empty = uri.AbsolutePath;
			}
			else
			{
				int num = uri.AbsolutePath.IndexOf('/', 1);
				if (-1 != num)
				{
					this.AccountName = uri.AbsolutePath.Substring(1, num - 1);
					empty = uri.AbsolutePath.Substring(num);
				}
				else
				{
					this.AccountName = uri.AbsolutePath.Substring(1);
				}
				this.BaseAddress = string.Format("{0}/{1}", uri.GetLeftPart(UriPartial.Authority), this.AccountName);
			}
			if (string.IsNullOrEmpty(this.AccountName))
			{
				throw new ArgumentException(string.Format(Resources.CannotParseAccountFromUriException, location), "location");
			}
			string str = string.Empty;
			string empty1 = string.Empty;
			if (!string.IsNullOrEmpty(empty))
			{
				int num1 = empty.IndexOf('/', 1);
				if (-1 != num1)
				{
					str = empty.Substring(1, num1 - 1);
					empty1 = empty.Substring(num1 + 1);
				}
				else
				{
					str = empty.Substring(1);
				}
			}
			if (string.IsNullOrEmpty(str))
			{
				str = "$root";
			}
			if (str.Equals("$logs"))
			{
				if (!isSourceLocation)
				{
					CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
					string containerOnlyValidForSourceException = Resources.ContainerOnlyValidForSourceException;
					object[] objArray2 = new object[] { "$logs" };
					throw new ArgumentException(string.Format(invariantCulture1, containerOnlyValidForSourceException, objArray2));
				}
			}
			else if (!Regex.IsMatch(str, "^\\$root$|^[a-z0-9]([a-z0-9]|(?<=[a-z0-9])-(?=[a-z0-9])){2,62}$"))
			{
				throw new ArgumentException("Invalid container name", "containerName");
			}
			if (!string.IsNullOrEmpty(empty1) && !empty1.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				empty1 = string.Concat(empty1, '/');
			}
			if (str.Equals("$root") && !string.IsNullOrEmpty(empty1))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.SubfoldersNotAllowedUnderRootContainerException, new object[0]));
			}
			this.ContainerName = str;
			this.Folder = Uri.UnescapeDataString(empty1);
			if (!string.IsNullOrEmpty(storageKey))
			{
				this.StorageCredential = new StorageCredentials(this.AccountName, storageKey);
			}
			else if (string.IsNullOrEmpty(containerSAS))
			{
				this.StorageCredential = new StorageCredentials();
			}
			else
			{
				this.StorageCredential = new StorageCredentials(containerSAS);
			}
			this.BlobContainer = new CloudBlobContainer(new Uri(string.Format("{0}/{1}", this.BaseAddress, this.ContainerName)), this.StorageCredential);
			this.transferOptions = transferOptions;
		}

		public IEnumerable<FileEntry> EnumerateLocation(IEnumerable<string> filePatterns, bool recursive, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource)
		{
			string str;
			bool flag;
			bool flag1;
			CancellationChecker cancellationChecker = new CancellationChecker();
			CancellationToken token = cancellationTokenSource.Token;
			CancellationTokenRegistration cancellationTokenRegistration = token.Register(new Action(cancellationChecker.Cancel));
			try
			{
				IEnumerable<string> filePatternWithDefault = this.GetFilePatternWithDefault(filePatterns);
				string str1 = string.Format("/{0}/{1}", this.ContainerName, this.Folder);
				if (this.EndPointPathStyle)
				{
					str1 = string.Concat("/", this.AccountName, str1);
				}
				HashSet<string> strs = new HashSet<string>();
				int num = this.GetMaxFileNameLength();
				foreach (string str2 in filePatternWithDefault)
				{
					if (str2.Length > num)
					{
						continue;
					}
					char[] charArray = str2.ToLowerInvariant().ToCharArray();
					int length = str2.Length;
					int num1 = 0;
					while (num1 < length && !char.IsLower(charArray[num1]))
					{
						num1++;
					}
					string str3 = str2.Substring(0, num1);
					if (num1 != length)
					{
						char chr = charArray[num1];
						strs.Add(string.Concat(str3, chr));
						strs.Add(string.Concat(str3, char.ToUpperInvariant(chr)));
					}
					else
					{
						strs.Add(str3);
					}
				}
				BlobRequestOptions blobRequestOptions = this.transferOptions.GetBlobRequestOptions(BlobRequestOperation.ListBlobs);
				OperationContext operationContext1 = new OperationContext();
				operationContext1.ClientRequestID=this.transferOptions.GetClientRequestId();
				OperationContext operationContext = operationContext1;
				HashSet<string> strs1 = new HashSet<string>();
				foreach (string str4 in strs)
				{
					BlobContinuationToken blobContinuationToken = null;
					do
					{
						BlobResultSegment blobResultSegment = null;
						ErrorFileEntry errorFileEntry = null;
						cancellationChecker.CheckCancellation();
						try
						{
                            blobResultSegment = this.BlobContainer.ListBlobsSegmented(string.Format("{0}{1}", this.Folder, str4), true, BlobListingDetails.Snapshots, new int?(250), blobContinuationToken, blobRequestOptions, operationContext);
						}
						catch (Exception exception)
						{
							errorFileEntry = new ErrorFileEntry(exception);
						}
						if (errorFileEntry == null)
						{
							blobContinuationToken = blobResultSegment.ContinuationToken;
						Label1:
							foreach (IListBlobItem result in blobResultSegment.Results)
							{
								cancellationChecker.CheckCancellation();
								ICloudBlob cloudBlob = result as ICloudBlob;
								if (cloudBlob == null)
								{
									continue;
								}
								string str5 = Uri.UnescapeDataString(cloudBlob.Uri.AbsolutePath);
								string fileName = Utils.AppendSnapShotToFileName(str5, cloudBlob.SnapshotTime);
								string str6 = fileName;
								str = (cloudBlob.SnapshotTime.HasValue ? "-S" : "-B");
								fileName = string.Concat(str6, str);
								if (strs1.Contains(fileName))
								{
									continue;
								}
								strs1.Add(fileName);
								using (IEnumerator<string> enumerator = filePatternWithDefault.GetEnumerator())
								{
									do
									{
										if (!enumerator.MoveNext())
										{
											goto Label1;
										}
										string str7 = string.Concat(str1, enumerator.Current);
										flag = (recursive ? str5.StartsWith(str7, StringComparison.OrdinalIgnoreCase) : str5.Equals(str7, StringComparison.OrdinalIgnoreCase));
										flag1 = flag;
									}
									while (!flag1);
									yield return new AzureFileEntry(str5.Remove(0, str1.Length), cloudBlob);
								}
							}
						}
						else
						{
							yield return errorFileEntry;
							goto Label0;
						}
					}
					while (blobContinuationToken != null);
				}
			}
			finally
			{
				((IDisposable)cancellationTokenRegistration).Dispose();
			}
		Label0:
			yield break;
		}

		public string GetAbsoluteUri(string relativePath)
		{
			return string.Format("{0}/{1}", this.BlobContainer.Uri, Uri.EscapeDataString(this.GetPathUnderContainer(relativePath)));
		}

		public ICloudBlob GetBlobObject(string relativePath, BlobType blobType)
		{
			return this.GetBlobObject(relativePath, null, blobType);
		}

		public ICloudBlob GetBlobObject(string relativePath, DateTimeOffset? snapshotTime, BlobType blobType)
		{
			string pathUnderContainer = this.GetPathUnderContainer(relativePath);
            if (BlobType.PageBlob == blobType)
			{
				return this.BlobContainer.GetPageBlobReference(pathUnderContainer, snapshotTime);
			}
			return this.BlobContainer.GetBlockBlobReference(pathUnderContainer, snapshotTime);
		}

		public IEnumerable<string> GetFilePatternWithDefault(IEnumerable<string> filePatterns)
		{
			if (filePatterns != null && filePatterns.Any<string>())
			{
				return filePatterns;
			}
			return new List<string>()
			{
				string.Empty
			};
		}

		public int GetMaxFileNameLength()
		{
			return 1024 - this.Folder.Length;
		}

		public string GetPathUnderContainer(string relativePath)
		{
			return string.Format("{0}{1}", this.Folder, relativePath);
		}
	}
}