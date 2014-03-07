using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="LoadBalancedEndpointList", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LoadBalancedEndpointList : List<InputEndpoint>
	{
		public LoadBalancedEndpointList()
		{
		}
	}
}