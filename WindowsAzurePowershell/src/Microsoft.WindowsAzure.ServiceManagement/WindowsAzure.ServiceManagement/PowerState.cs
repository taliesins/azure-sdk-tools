using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum PowerState
	{
		[EnumMember]
		Unknown,
		[EnumMember]
		Starting,
		[EnumMember]
		Started,
		[EnumMember]
		Stopping,
		[EnumMember]
		Stopped
	}
}