using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StartRolesOperation : RoleSetOperation
	{
		public override string OperationType
		{
			get
			{
				return "StartRolesOperation";
			}
			set
			{
			}
		}

		public StartRolesOperation()
		{
		}
	}
}