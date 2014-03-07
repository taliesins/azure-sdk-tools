using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="ExtensionImages", ItemName="ExtensionImage", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ExtensionImageList : List<ExtensionImage>
	{
		public ExtensionImageList()
		{
		}

		public ExtensionImageList(IEnumerable<ExtensionImage> extensions) : base(extensions)
		{
		}
	}
}