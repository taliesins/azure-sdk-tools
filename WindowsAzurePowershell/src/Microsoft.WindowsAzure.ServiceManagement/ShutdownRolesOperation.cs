using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ShutdownRolesOperation : RoleSetOperation
	{
		public override string OperationType
		{
			get
			{
				return "ShutdownRolesOperation";
			}
			set
			{
			}
		}

		[DataMember(EmitDefaultValue=false, Order=0)]
		public Microsoft.WindowsAzure.ServiceManagement.PostShutdownAction PostShutdownAction
		{
			get;
			set;
		}

		public ShutdownRolesOperation()
		{
		}
	}
}