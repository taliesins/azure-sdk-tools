using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="ReplicationProgressList", ItemName="ReplicationProgressElement", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ReplicationProgressList : Collection<ReplicationProgressElement>
	{
		public ReplicationProgressList()
		{
		}
	}
}