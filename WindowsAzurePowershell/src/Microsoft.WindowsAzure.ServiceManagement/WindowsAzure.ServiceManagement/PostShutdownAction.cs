using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum PostShutdownAction
	{
		[EnumMember]
		Stopped,
		[EnumMember]
		StoppedDeallocated,
		[EnumMember]
		Undefined
	}
}