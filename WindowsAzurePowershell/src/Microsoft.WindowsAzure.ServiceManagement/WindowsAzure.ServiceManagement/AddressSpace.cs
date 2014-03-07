using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AddressSpace : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=1)]
		public AddressPrefixList AddressPrefixes
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public AddressSpace()
		{
		}
	}
}