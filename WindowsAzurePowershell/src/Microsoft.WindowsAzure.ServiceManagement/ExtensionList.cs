using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Extensions", ItemName="Extension", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ExtensionList : List<Extension>
	{
		public ExtensionList()
		{
		}

		public ExtensionList(IEnumerable<Extension> list) : base(list)
		{
		}
	}
}