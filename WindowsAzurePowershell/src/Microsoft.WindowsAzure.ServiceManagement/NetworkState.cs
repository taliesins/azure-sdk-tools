using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum NetworkState
	{
		[EnumMember]
		Created,
		[EnumMember]
		Creating,
		[EnumMember]
		Updating,
		[EnumMember]
		Deleting,
		[EnumMember]
		Unavailable
	}
}