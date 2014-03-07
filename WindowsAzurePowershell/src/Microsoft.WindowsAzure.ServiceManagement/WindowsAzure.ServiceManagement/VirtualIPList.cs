using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="VirtualIPs", ItemName="VirtualIP", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualIPList : List<VirtualIP>
	{
		public VirtualIPList()
		{
		}

		public VirtualIPList(IEnumerable<VirtualIP> ips) : base(ips)
		{
		}
	}
}