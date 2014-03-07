using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	[KnownType(typeof(LinuxProvisioningConfigurationSet))]
	[KnownType(typeof(NetworkConfigurationSet))]
	[KnownType(typeof(ProvisioningConfigurationSet))]
	[KnownType(typeof(WindowsProvisioningConfigurationSet))]
	public class ConfigurationSet : Mergable<ConfigurationSet>
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public virtual string ConfigurationSetType
		{
			get;
			set;
		}

		protected ConfigurationSet()
		{
		}

		public override object ResolveType()
		{
			if (base.GetType() != typeof(ConfigurationSet))
			{
				return this;
			}
			if (!string.IsNullOrEmpty(this.ConfigurationSetType))
			{
				if (string.Equals(this.ConfigurationSetType, "WindowsProvisioningConfiguration"))
				{
					return base.Convert<WindowsProvisioningConfigurationSet>();
				}
				if (string.Equals(this.ConfigurationSetType, "LinuxProvisioningConfiguration"))
				{
					return base.Convert<LinuxProvisioningConfigurationSet>();
				}
				if (string.Equals(this.ConfigurationSetType, "NetworkConfiguration"))
				{
					return base.Convert<NetworkConfigurationSet>();
				}
			}
			return this;
		}
	}
}