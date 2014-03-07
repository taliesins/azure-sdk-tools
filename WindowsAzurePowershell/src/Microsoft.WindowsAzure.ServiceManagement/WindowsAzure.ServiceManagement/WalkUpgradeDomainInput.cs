using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="WalkUpgradeDomain", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class WalkUpgradeDomainInput : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public int UpgradeDomain
		{
			get;
			set;
		}

		public WalkUpgradeDomainInput()
		{
		}
	}
}