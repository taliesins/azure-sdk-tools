using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="NamedRoles", ItemName="Role", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class NamedRoles : List<RoleExtensions>
	{
		public NamedRoles()
		{
		}

		public NamedRoles(IEnumerable<RoleExtensions> list) : base(list)
		{
		}
	}
}