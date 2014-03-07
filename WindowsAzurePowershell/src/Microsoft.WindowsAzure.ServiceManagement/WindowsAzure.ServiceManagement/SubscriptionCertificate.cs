using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubscriptionCertificate : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=3)]
		public DateTime Created
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string SubscriptionCertificateData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=0)]
		public string SubscriptionCertificatePublicKey
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string SubscriptionCertificateThumbprint
		{
			get;
			set;
		}

		public SubscriptionCertificate()
		{
		}
	}
}