using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferCallbacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferRecursiveTransferOptions
	{
		private string sourceKey;

		private string sourceSAS;

		private string destinationKey;

		private string destinationSAS;

		private BlobType uploadBlobType;

		public BlobTransferBeforeQueueCallback BeforeQueueCallback
		{
			get;
			set;
		}

		public string DestinationKey
		{
			get
			{
				return this.destinationKey;
			}
			set
			{
				try
				{
					Convert.FromBase64String(value);
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					throw new ArgumentException(string.Format(Resources.StorageKeyInvalidFormatException, "DestinationKey"), formatException);
				}
				if (!string.IsNullOrEmpty(this.destinationSAS) && !string.IsNullOrEmpty(value))
				{
					throw new ArgumentException(string.Format(Resources.CanOnlySetOneCredentialException, "DestinationKey", "DestinationSAS"));
				}
				this.destinationKey = value;
			}
		}

		public string DestinationSAS
		{
			get
			{
				return this.destinationSAS;
			}
			set
			{
				if (!string.IsNullOrEmpty(this.destinationKey) && !string.IsNullOrEmpty(value))
				{
					throw new ArgumentException(string.Format(Resources.CanOnlySetOneCredentialException, "DestinationKey", "DestinationSAS"));
				}
				this.destinationSAS = value;
			}
		}

		public bool DownloadCheckMd5
		{
			get;
			set;
		}

		public FileAttributes ExcludedAttributes
		{
			get;
			set;
		}

		public bool ExcludeNewer
		{
			get;
			set;
		}

		public bool ExcludeOlder
		{
			get;
			set;
		}

		public bool FakeTransfer
		{
			get;
			set;
		}

		public IEnumerable<string> FilePatterns
		{
			get;
			set;
		}

		public BlobTransferFileTransferStatus FileTransferStatus
		{
			get;
			set;
		}

		public FileAttributes IncludedAttributes
		{
			get;
			set;
		}

		public bool MoveFile
		{
			get;
			set;
		}

		public bool OnlyFilesWithArchiveBit
		{
			get;
			set;
		}

		public bool Recursive
		{
			get;
			set;
		}

		public bool RestartableMode
		{
			get;
			set;
		}

		public string SourceKey
		{
			get
			{
				return this.sourceKey;
			}
			set
			{
				try
				{
					Convert.FromBase64String(value);
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					throw new ArgumentException(string.Format(Resources.StorageKeyInvalidFormatException, "SourceKey"), formatException);
				}
				if (!string.IsNullOrEmpty(this.sourceSAS) && !string.IsNullOrEmpty(value))
				{
					throw new ArgumentException(string.Format(Resources.CanOnlySetOneCredentialException, "SourceKey", "SourceSAS"));
				}
				this.sourceKey = value;
			}
		}

		public string SourceSAS
		{
			get
			{
				return this.sourceSAS;
			}
			set
			{
				if (!string.IsNullOrEmpty(this.sourceKey) && !string.IsNullOrEmpty(value))
				{
					throw new ArgumentException(string.Format(Resources.CanOnlySetOneCredentialException, "SourceKey", "SourceSAS"));
				}
				this.sourceSAS = value;
			}
		}

		public bool TransferSnapshots
		{
			get;
			set;
		}

		public BlobType UploadBlobType
		{
			get
			{
				return this.uploadBlobType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BlobType), value))
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string undefinedBlobTypeException = Resources.UndefinedBlobTypeException;
					object[] objArray = new object[] { value };
					throw new ArgumentException(string.Format(invariantCulture, undefinedBlobTypeException, objArray));
				}
				this.uploadBlobType = value;
			}
		}

		public BlobTransferRecursiveTransferOptions()
		{
			this.SourceKey = string.Empty;
			this.SourceSAS = string.Empty;
			this.DestinationKey = string.Empty;
			this.DestinationSAS = string.Empty;
			this.Recursive = true;
			this.FakeTransfer = false;
			this.ExcludeNewer = false;
			this.ExcludeOlder = false;
			this.OnlyFilesWithArchiveBit = false;
			this.UploadBlobType = BlobType.PageBlob;
		}
	}
}