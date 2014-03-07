using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Certificate : IExtensibleDataObject
	{
		[DataMember(Order=1, EmitDefaultValue=false)]
		public Uri CertificateUrl;

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string Thumbprint;

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string ThumbprintAlgorithm;

		[DataMember(Order=4, EmitDefaultValue=false)]
		public string Data;

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public Certificate()
		{
		}
	}
}