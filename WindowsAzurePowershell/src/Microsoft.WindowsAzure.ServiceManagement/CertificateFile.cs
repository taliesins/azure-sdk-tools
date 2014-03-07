using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CertificateFile : IExtensibleDataObject
	{
		[DataMember(Order=1)]
		public string Data;

		[DataMember(Order=2)]
		public string CertificateFormat
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public string Password
		{
			get;
			set;
		}

		public CertificateFile()
		{
		}
	}
}