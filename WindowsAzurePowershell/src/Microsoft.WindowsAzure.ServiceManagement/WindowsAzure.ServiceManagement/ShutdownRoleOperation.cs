using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ShutdownRoleOperation : RoleOperation
	{
		public override string OperationType
		{
			get
			{
				return "ShutdownRoleOperation";
			}
			set
			{
			}
		}

		[DataMember(EmitDefaultValue=false, Order=0)]
		public Microsoft.WindowsAzure.ServiceManagement.PostShutdownAction? PostShutdownAction
		{
			get;
			set;
		}

		public ShutdownRoleOperation()
		{
		}
	}
}