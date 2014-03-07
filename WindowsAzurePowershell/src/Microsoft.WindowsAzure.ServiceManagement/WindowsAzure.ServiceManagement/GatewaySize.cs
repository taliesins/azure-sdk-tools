using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum GatewaySize
	{
		[EnumMember]
		Small,
		[EnumMember]
		Medium,
		[EnumMember]
		Large,
		[EnumMember]
		ExtraLarge
	}
}