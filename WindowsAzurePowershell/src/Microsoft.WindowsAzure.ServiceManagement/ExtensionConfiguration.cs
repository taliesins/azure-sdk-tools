using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="ExtensionConfiguration", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ExtensionConfiguration : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=1)]
		public Microsoft.WindowsAzure.ServiceManagement.AllRoles AllRoles;

		[DataMember(EmitDefaultValue=false, Order=2)]
		public Microsoft.WindowsAzure.ServiceManagement.NamedRoles NamedRoles;

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public ExtensionConfiguration()
		{
		}
	}
}