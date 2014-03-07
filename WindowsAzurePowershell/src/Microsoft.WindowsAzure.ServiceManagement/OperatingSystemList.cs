using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="OperatingSystems", ItemName="OperatingSystem", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystemList : List<Microsoft.WindowsAzure.ServiceManagement.OperatingSystem>
	{
		public OperatingSystemList()
		{
		}

		public OperatingSystemList(IEnumerable<Microsoft.WindowsAzure.ServiceManagement.OperatingSystem> operatingSystems) : base(operatingSystems)
		{
		}
	}
}