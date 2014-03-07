using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LocalNetworkSite : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=2)]
		public Microsoft.WindowsAzure.ServiceManagement.AddressSpace AddressSpace
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public ConnectionList Connections
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string VpnGatewayAddress
		{
			get;
			set;
		}

		public LocalNetworkSite()
		{
		}
	}
}