using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="AvailabilityResponse", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AvailabilityResponse : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Reason
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public bool Result
		{
			get;
			set;
		}

		public AvailabilityResponse()
		{
		}
	}
}