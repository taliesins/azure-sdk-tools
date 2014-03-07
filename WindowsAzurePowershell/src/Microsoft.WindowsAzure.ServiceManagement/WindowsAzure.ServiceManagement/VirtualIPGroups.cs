using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="VirtualIPGroups", ItemName="VirtualIPGroup", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualIPGroups : List<VirtualIPGroup>
	{
		public VirtualIPGroups()
		{
		}

		public VirtualIPGroups(IEnumerable<VirtualIPGroup> groups) : base(groups)
		{
		}
	}
}