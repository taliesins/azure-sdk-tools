using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Regions", ItemName="Region", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RegionList : List<string>
	{
		public RegionList()
		{
		}
	}
}