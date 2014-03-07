using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="AllRoles", ItemName="Extension", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AllRoles : List<Extension>
	{
		public AllRoles()
		{
		}

		public AllRoles(IEnumerable<Extension> list) : base(list)
		{
		}
	}
}