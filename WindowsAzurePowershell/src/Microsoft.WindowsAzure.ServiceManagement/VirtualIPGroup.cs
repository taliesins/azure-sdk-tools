using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualIPGroup : IExtensibleDataObject
	{
		[DataMember(Order=2, EmitDefaultValue=false)]
		public EndpointContractList EndpointContracts
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public VirtualIPList VirtualIPs
		{
			get;
			set;
		}

		public VirtualIPGroup()
		{
		}
	}
}