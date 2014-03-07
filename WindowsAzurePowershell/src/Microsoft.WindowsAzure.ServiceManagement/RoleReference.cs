using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleReference : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public string DeploymentName
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string HostedServiceName
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string RoleName
		{
			get;
			set;
		}

		public RoleReference()
		{
		}

		public override bool Equals(object obj)
		{
			RoleReference roleReference = obj as RoleReference;
			if (roleReference == null)
			{
				return false;
			}
			if (!string.Equals(roleReference.HostedServiceName, this.HostedServiceName, StringComparison.OrdinalIgnoreCase) || !string.Equals(roleReference.DeploymentName, this.DeploymentName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return string.Equals(roleReference.RoleName, this.RoleName, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			string str = string.Format("{0}-{1}-{2}", this.HostedServiceName, this.DeploymentName, this.RoleName);
			return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
		}
	}
}