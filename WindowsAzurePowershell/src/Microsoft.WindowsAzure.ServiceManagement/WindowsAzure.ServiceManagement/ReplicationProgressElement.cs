using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ReplicationProgressElement : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Progress
		{
			get;
			set;
		}

		public ReplicationProgressElement()
		{
		}
	}
}