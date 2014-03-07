using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ReplicationInput : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=0, EmitDefaultValue=false)]
		public RegionList TargetLocations
		{
			get;
			set;
		}

		public ReplicationInput()
		{
		}
	}
}