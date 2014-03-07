using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RestartRoleOperation : RoleOperation
	{
		public override string OperationType
		{
			get
			{
				return "RestartRoleOperation";
			}
			set
			{
			}
		}

		public RestartRoleOperation()
		{
		}
	}
}