using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="RoleList", ItemName="Role", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleList : List<Role>
	{
		public RoleList()
		{
		}

		public RoleList(IEnumerable<Role> roles) : base(roles)
		{
		}
	}
}