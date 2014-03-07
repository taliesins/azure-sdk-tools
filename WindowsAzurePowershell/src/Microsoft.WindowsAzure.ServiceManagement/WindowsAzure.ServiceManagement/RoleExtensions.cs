using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleExtensions : IExtensibleDataObject
	{
		[DataMember(Order=1)]
		public string RoleName;

		[DataMember(Order=2)]
		public ExtensionList Extensions;

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public RoleExtensions()
		{
		}
	}
}