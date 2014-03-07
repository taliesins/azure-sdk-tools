using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StartRoleOperation : RoleOperation
	{
		public override string OperationType
		{
			get
			{
				return "StartRoleOperation";
			}
			set
			{
			}
		}

		public StartRoleOperation()
		{
		}
	}
}