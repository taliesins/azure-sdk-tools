using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	[KnownType(typeof(PersistentVMRole))]
	public class Role : Mergable<PersistentVMRole>
	{
		[DataMember(Name="ConfigurationSets", EmitDefaultValue=false, Order=4)]
		public Collection<ConfigurationSet> ConfigurationSets
		{
			get
			{
				return base.GetValue<Collection<ConfigurationSet>>("ConfigurationSets");
			}
			set
			{
				base.SetValue<Collection<ConfigurationSet>>("ConfigurationSets", value);
			}
		}

		public Microsoft.WindowsAzure.ServiceManagement.NetworkConfigurationSet NetworkConfigurationSet
		{
			get
			{
				if (this.ConfigurationSets == null)
				{
					return null;
				}
				return this.ConfigurationSets.FirstOrDefault<ConfigurationSet>((ConfigurationSet cset) => cset is Microsoft.WindowsAzure.ServiceManagement.NetworkConfigurationSet) as Microsoft.WindowsAzure.ServiceManagement.NetworkConfigurationSet;
			}
			set
			{
				if (this.ConfigurationSets == null)
				{
					this.ConfigurationSets = new Collection<ConfigurationSet>();
				}
				Microsoft.WindowsAzure.ServiceManagement.NetworkConfigurationSet networkConfigurationSet = this.ConfigurationSets.FirstOrDefault<ConfigurationSet>((ConfigurationSet cset) => cset is Microsoft.WindowsAzure.ServiceManagement.NetworkConfigurationSet) as Microsoft.WindowsAzure.ServiceManagement.NetworkConfigurationSet;
				if (networkConfigurationSet != null)
				{
					this.ConfigurationSets.Remove(networkConfigurationSet);
				}
				this.ConfigurationSets.Add(value);
			}
		}

		[DataMember(Order=2)]
		public string OsVersion
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public virtual string RoleName
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public virtual string RoleType
		{
			get;
			set;
		}

		public Role()
		{
		}

		public override object ResolveType()
		{
			if (base.GetType() != typeof(Role))
			{
				return this;
			}
			if (this.RoleType != typeof(PersistentVMRole).Name)
			{
				return this;
			}
			return base.Convert<PersistentVMRole>();
		}
	}
}