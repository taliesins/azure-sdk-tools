using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CustomDomain : IExtensibleDataObject
	{
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

		[DataMember(EmitDefaultValue=false, Order=2)]
		public bool UseSubDomainName
		{
			get;
			set;
		}

		public CustomDomain()
		{
		}
	}
}