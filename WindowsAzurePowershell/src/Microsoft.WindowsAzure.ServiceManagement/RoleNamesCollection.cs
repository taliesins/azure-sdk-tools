using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Roles", ItemName="Name", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleNamesCollection : Collection<string>
	{
		public RoleNamesCollection()
		{
		}
	}
}