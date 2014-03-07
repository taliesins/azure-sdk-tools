using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="Extensions", ItemName="Extension")]
	public class HostedServiceExtensionList : List<HostedServiceExtension>
	{
		public HostedServiceExtensionList()
		{
		}

		public HostedServiceExtensionList(IEnumerable<HostedServiceExtension> extensions) : base(extensions)
		{
		}
	}
}