using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferFileTransferEntry
	{
		private const int FormatVersion = 1;

		private const int EnumTypeSize = 2;

		private const int OffsetSize = 8;

		private const int CopyIdSize = 72;

		private const int ETagSize = 38;

		private const int BlockIdPrefixSize = 18;

		private const int StaticPartSize = 1206;

		private const int StringPartPosition = 1058;

		private const int AllocateGranularity = 128;

		private readonly Encoding encoding = Encoding.Unicode;

		private volatile BlobTransferEntryStatus status;

		private string blockIdPrefix;

		private BlobTransferFileTransferEntry.DeletionBlobSet blobSet;

		private object entryLock = new object();

		internal BlobTransferFileTransferEntry.DeletionBlobSet BlobSet
		{
			get
			{
				return this.blobSet;
			}
			set
			{
				if (this.blobSet != null)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string transferEntryPropertyCanBeSetOnlyOnceException = Resources.TransferEntryPropertyCanBeSetOnlyOnceException;
					object[] objArray = new object[] { "BlobSet" };
					throw new InvalidOperationException(string.Format(invariantCulture, transferEntryPropertyCanBeSetOnlyOnceException, objArray));
				}
				this.blobSet = value;
			}
		}

		public string BlockIdPrefix
		{
			get
			{
				return this.blockIdPrefix;
			}
			internal set
			{
				if (this.blockIdPrefix != null)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string transferEntryPropertyCanBeSetOnlyOnceException = Resources.TransferEntryPropertyCanBeSetOnlyOnceException;
					object[] objArray = new object[] { "BlockIdPrefix" };
					throw new ArgumentException(string.Format(invariantCulture, transferEntryPropertyCanBeSetOnlyOnceException, objArray));
				}
				this.blockIdPrefix = value;
			}
		}

		internal BlobTransferFileTransferEntryCheckPoint CheckPoint
		{
			get;
			set;
		}

		public string CopyId
		{
			get;
			internal set;
		}

		public string DestinationRelativePath
		{
			get;
			private set;
		}

		internal object EntryLock
		{
			get
			{
				return this.entryLock;
			}
		}

		public bool EntryTransferFinished
		{
			get
			{
				return BlobTransferEntryStatus.Finished == this.Status;
			}
		}

		public string ETag
		{
			get;
			internal set;
		}

		public DateTimeOffset? LastModified
		{
			get;
			internal set;
		}

		public BlobType SourceBlobType
		{
			get;
			private set;
		}

		public string SourceRelativePath
		{
			get;
			private set;
		}

		public DateTimeOffset? SourceSnapshotTime
		{
			get;
			private set;
		}

		public BlobTransferEntryStatus Status
		{
			get
			{
				return (BlobTransferEntryStatus)this.status;
			}
			internal set
			{
				if (!Enum.IsDefined(typeof(BlobTransferEntryStatus), value))
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string undefinedTransferEntryStatusException = Resources.UndefinedTransferEntryStatusException;
					object[] objArray = new object[] { value };
					throw new ArgumentException(string.Format(invariantCulture, undefinedTransferEntryStatusException, objArray));
				}
				this.status = value;
			}
		}

		public BlobTransferFileTransferEntry(byte[] infoBuffer)
		{
			this.ReadFromBuffer(infoBuffer);
		}

		public BlobTransferFileTransferEntry(string sourceRelativePath, string destinationRelativePath, DateTimeOffset? lastModified, DateTimeOffset? sourceSnapshotTime, BlobType sourceBlobType, string copyId, string etag, string blockIdPrefix, BlobTransferEntryStatus status, BlobTransferFileTransferEntryCheckPoint checkPoint)
		{
			this.CopyId = copyId;
			this.ETag = etag;
			this.blockIdPrefix = blockIdPrefix;
			this.SourceRelativePath = sourceRelativePath;
			this.DestinationRelativePath = destinationRelativePath;
			this.LastModified = lastModified;
			this.SourceSnapshotTime = sourceSnapshotTime;
			this.SourceBlobType = sourceBlobType;
			this.Status = BlobTransferEntryStatus.NotStarted;
			this.CheckPoint = (checkPoint == null ? new BlobTransferFileTransferEntryCheckPoint() : checkPoint);
		}

		internal BlobTransferFileTransferEntry(string sourceRelativePath, string destinationRelativePath, DateTimeOffset? lastModified, DateTimeOffset? sourceSnapshotTime, BlobType sourceBlobType) : this(sourceRelativePath, destinationRelativePath, lastModified, sourceSnapshotTime, sourceBlobType, null, null, null, BlobTransferEntryStatus.NotStarted, null)
		{
		}

		internal BlobTransferFileTransferEntry() : this(null, null, null, null, 0)
		{
		}

		private void AllocateBuffer(ref byte[] buffer, int length)
		{
			if (buffer == null || (int)buffer.Length < length)
			{
				int num = (length / 128 + 1) * 128;
				buffer = new byte[num];
			}
		}

		private int CalculateBufferSize()
		{
			return 1206 + (int)this.encoding.GetBytes(this.SourceRelativePath).Length + (int)this.encoding.GetBytes(this.DestinationRelativePath).Length;
		}

		public BlobTransferFileTransferEntryCheckPoint GetCheckPoint()
		{
			BlobTransferFileTransferEntryCheckPoint blobTransferFileTransferEntryCheckPoint;
			lock (this.entryLock)
			{
				blobTransferFileTransferEntryCheckPoint = new BlobTransferFileTransferEntryCheckPoint(this.CheckPoint);
			}
			return blobTransferFileTransferEntryCheckPoint;
		}

		public void GetInfoBuffer(ref byte[] entryBuffer, out int infoLength)
		{
			infoLength = this.CalculateBufferSize();
			this.AllocateBuffer(ref entryBuffer, infoLength);
			MemoryStream memoryStream = new MemoryStream(entryBuffer)
			{
				Position = (long)0
			};
			memoryStream.Write(BitConverter.GetBytes(1), 0, 4);
			memoryStream.Write(BitConverter.GetBytes((short)this.status), 0, 2);
			memoryStream.Write(BitConverter.GetBytes((short)this.SourceBlobType), 0, 2);
			this.WriteDateTime(memoryStream, this.LastModified);
			this.WriteDateTime(memoryStream, this.SourceSnapshotTime);
			BlobTransferFileTransferEntryCheckPoint checkPoint = this.GetCheckPoint();
			memoryStream.Write(BitConverter.GetBytes(checkPoint.EntryTransferOffset), 0, 8);
			if (checkPoint.UploadWindow == null)
			{
				memoryStream.Write(BitConverter.GetBytes((short)0), 0, 2);
			}
			else
			{
				memoryStream.Write(BitConverter.GetBytes((short)checkPoint.UploadWindow.Count), 0, 2);
				foreach (long uploadWindow in checkPoint.UploadWindow)
				{
					memoryStream.Write(BitConverter.GetBytes(uploadWindow), 0, 8);
				}
			}
			memoryStream.Position = (long)1058;
			Array.Clear(entryBuffer, 1058, infoLength - 1058);
			this.WriteString(memoryStream, this.CopyId, 72);
			this.WriteString(memoryStream, this.ETag, 38);
			this.WriteString(memoryStream, this.blockIdPrefix, 18);
			this.WriteString(memoryStream, this.SourceRelativePath, -1);
			this.WriteString(memoryStream, this.DestinationRelativePath, -1);
		}

		private DateTimeOffset? ReadDateTime(byte[] membuff, ref int offset)
		{
			long num = BitConverter.ToInt64(membuff, offset);
			offset = offset + 8;
			if ((long)-1 == num)
			{
				return null;
			}
			return new DateTimeOffset?(DateTimeOffset.FromFileTime(num));
		}

		private void ReadFromBuffer(byte[] entryBuffer)
		{
			int num = 0;
			if (1 != BitConverter.ToInt32(entryBuffer, num))
			{
				throw new InvalidDataException(Resources.RestartableInfoCorruptedException);
			}
			num = num + 4;
			this.status = (BlobTransferEntryStatus)Enum.ToObject(typeof(BlobTransferEntryStatus), BitConverter.ToInt16(entryBuffer, num));
			num = num + 2;
			this.SourceBlobType = (BlobType)Enum.ToObject(typeof(BlobType), BitConverter.ToInt16(entryBuffer, num));
			num = num + 2;
			this.LastModified = this.ReadDateTime(entryBuffer, ref num);
			this.SourceSnapshotTime = this.ReadDateTime(entryBuffer, ref num);
			long num1 = BitConverter.ToInt64(entryBuffer, num);
			num = num + 8;
			short num2 = BitConverter.ToInt16(entryBuffer, num);
			num = num + 2;
			if (num2 > 128)
			{
				throw new InvalidDataException(Resources.RestartableInfoCorruptedException);
			}
			List<long> nums = null;
			if (num2 > 0)
			{
				nums = new List<long>();
				while (num2 > 0)
				{
					nums.Add(BitConverter.ToInt64(entryBuffer, num));
					num = num + 8;
					num2 = (short)(num2 - 1);
				}
			}
			this.CheckPoint = new BlobTransferFileTransferEntryCheckPoint(num1, nums);
			num = 1058;
			this.CopyId = this.ReadString(entryBuffer, ref num, 72);
			this.ETag = this.ReadString(entryBuffer, ref num, 38);
			this.blockIdPrefix = this.ReadString(entryBuffer, ref num, 18);
			this.SourceRelativePath = this.ReadString(entryBuffer, ref num, -1);
			this.DestinationRelativePath = this.ReadString(entryBuffer, ref num, -1);
		}

		private string ReadString(byte[] memBuffer, ref int position, int length)
		{
			int num = BitConverter.ToInt32(memBuffer, position);
			if (num <= 0)
			{
				throw new InvalidDataException(Resources.RestartableInfoCorruptedException);
			}
			position = position + 4;
			bool flag = true;
			for (int i = position; i < position + num; i++)
			{
				if (memBuffer[i] != 0)
				{
					flag = false;
				}
			}
			string str = null;
			if (!flag)
			{
				str = this.encoding.GetString(memBuffer, position, num);
			}
			position = position + num;
			return str;
		}

		private void WriteDateTime(MemoryStream ms, DateTimeOffset? input)
		{
			if (!input.HasValue)
			{
				ms.Write(BitConverter.GetBytes((long)-1), 0, 8);
				return;
			}
			DateTimeOffset valueOrDefault = input.GetValueOrDefault();
			ms.Write(BitConverter.GetBytes(valueOrDefault.ToFileTime()), 0, 8);
		}

		private void WriteString(MemoryStream ms, string input, int length)
		{
			if (input == null)
			{
				ms.Write(BitConverter.GetBytes(length), 0, 4);
				MemoryStream position = ms;
				position.Position = position.Position + (long)length;
				return;
			}
			byte[] bytes = this.encoding.GetBytes(input);
			ms.Write(BitConverter.GetBytes((int)bytes.Length), 0, 4);
			ms.Write(bytes, 0, (int)bytes.Length);
		}

		internal class DeletionBlobSet
		{
			public CountdownEvent CountDown
			{
				get;
				private set;
			}

			public ICloudBlob RootBlob
			{
				get;
				private set;
			}

			public DeletionBlobSet(ICloudBlob rootBlob, int count)
			{
				this.RootBlob = rootBlob;
				this.CountDown = new CountdownEvent(count);
			}
		}
	}
}