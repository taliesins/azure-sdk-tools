using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleSetOperation : Mergable<RoleSetOperation>
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public virtual string OperationType
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public virtual RoleNamesCollection Roles
		{
			get;
			set;
		}

		protected RoleSetOperation()
		{
		}
	}
}