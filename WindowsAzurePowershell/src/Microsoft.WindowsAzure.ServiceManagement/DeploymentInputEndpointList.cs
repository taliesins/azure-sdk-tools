using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="InputEndpointList", ItemName="InputEndpoint", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DeploymentInputEndpointList : List<DeploymentInputEndpoint>
	{
		public DeploymentInputEndpointList()
		{
		}

		public DeploymentInputEndpointList(IEnumerable<DeploymentInputEndpoint> endpoints) : base(endpoints)
		{
		}
	}
}