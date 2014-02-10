using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Storage.DataMovement.Exceptions
{
	[Serializable]
	public class BlobTransferException : Exception
	{
		private const int ExceptionVersion = 1;

		private const string VersionFieldName = "Version";

		private const string ErrorCodeFieldName = "ErrorCode";

		private BlobTransferErrorCode errorCode;

		public BlobTransferErrorCode ErrorCode
		{
			get
			{
				return this.errorCode;
			}
		}

		public BlobTransferException(BlobTransferErrorCode errorCode)
		{
			this.errorCode = errorCode;
		}

		public BlobTransferException(BlobTransferErrorCode errorCode, string message) : base(message)
		{
			this.errorCode = errorCode;
		}

		public BlobTransferException(BlobTransferErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
		{
			this.errorCode = errorCode;
		}

		private BlobTransferException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info.GetInt32("Version") >= 1)
			{
				this.errorCode = (BlobTransferErrorCode)info.GetInt32("ErrorCode");
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("Version", 1);
			info.AddValue("ErrorCode", this.errorCode);
			base.GetObjectData(info, context);
		}
	}
}