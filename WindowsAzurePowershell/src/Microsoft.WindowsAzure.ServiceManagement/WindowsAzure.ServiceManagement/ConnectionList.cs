using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Connections", ItemName="Connection", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ConnectionList : List<Connection>
	{
		public ConnectionList()
		{
		}
	}
}