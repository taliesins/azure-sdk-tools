using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="EndpointContracts", ItemName="EndpointContract")]
	public class EndpointContractList : List<EndpointContract>
	{
		public EndpointContractList()
		{
		}

		public EndpointContractList(IEnumerable<EndpointContract> collection) : base(collection)
		{
		}
	}
}