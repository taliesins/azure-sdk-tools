using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class InstanceEndpointList : List<InstanceEndpoint>
	{
		public InstanceEndpointList()
		{
		}
	}
}