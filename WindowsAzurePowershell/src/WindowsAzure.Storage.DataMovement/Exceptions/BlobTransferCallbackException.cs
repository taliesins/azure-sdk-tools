using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Storage.DataMovement.Exceptions
{
	[Serializable]
	public class BlobTransferCallbackException : Exception
	{
		private const int ExceptionVersion = 1;

		private const string VersionFieldName = "Version";

		public BlobTransferCallbackException(string message, Exception ex) : base(message, ex)
		{
		}

		private BlobTransferCallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("Version", 1);
			base.GetObjectData(info, context);
		}
	}
}