using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="CustomDomains", ItemName="CustomDomain")]
	public class CustomDomainList : List<CustomDomain>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public CustomDomainList()
		{
		}

		public CustomDomainList(IEnumerable<CustomDomain> customDomains) : base(customDomains)
		{
		}
	}
}