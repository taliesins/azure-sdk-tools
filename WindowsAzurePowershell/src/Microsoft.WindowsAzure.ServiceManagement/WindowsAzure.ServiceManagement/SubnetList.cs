using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Subnets", ItemName="Subnet", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubnetList : List<Subnet>
	{
		public SubnetList()
		{
		}
	}
}