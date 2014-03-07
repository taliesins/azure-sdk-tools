using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum PostCaptureAction
	{
		[EnumMember]
		Delete,
		[EnumMember]
		Reprovision
	}
}