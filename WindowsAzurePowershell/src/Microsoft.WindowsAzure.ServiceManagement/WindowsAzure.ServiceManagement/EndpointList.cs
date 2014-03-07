using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="Endpoints", ItemName="Endpoint")]
	public class EndpointList : List<string>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public EndpointList()
		{
		}
	}
}