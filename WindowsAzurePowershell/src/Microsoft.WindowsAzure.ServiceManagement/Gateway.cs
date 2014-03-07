using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Gateway : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Profile
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public LocalNetworkSiteList Sites
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public AddressSpace VPNClientAddressPool
		{
			get;
			set;
		}

		public Gateway()
		{
		}
	}
}