using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class NetworkConfigurationSet : ConfigurationSet
	{
		public override string ConfigurationSetType
		{
			get
			{
				return "NetworkConfiguration";
			}
			set
			{
				base.ConfigurationSetType = value;
			}
		}

		[DataMember(Name="InputEndpoints", EmitDefaultValue=false, Order=0)]
		public Collection<InputEndpoint> InputEndpoints
		{
			get
			{
				return base.GetValue<Collection<InputEndpoint>>("InputEndpoints");
			}
			set
			{
				base.SetValue<Collection<InputEndpoint>>("InputEndpoints", value);
			}
		}

		[DataMember(Name="SubnetNames", EmitDefaultValue=false, Order=1)]
		public SubnetNamesCollection SubnetNames
		{
			get
			{
				return base.GetValue<SubnetNamesCollection>("SubnetNames");
			}
			set
			{
				base.SetValue<SubnetNamesCollection>("SubnetNames", value);
			}
		}

		[DataMember(Name="VirtualIPGroups", EmitDefaultValue=false, Order=2)]
		public Microsoft.WindowsAzure.ServiceManagement.VirtualIPGroups VirtualIPGroups
		{
			get
			{
				return base.GetValue<Microsoft.WindowsAzure.ServiceManagement.VirtualIPGroups>("VirtualIPGroups");
			}
			set
			{
				base.SetValue<Microsoft.WindowsAzure.ServiceManagement.VirtualIPGroups>("VirtualIPGroups", value);
			}
		}

		public NetworkConfigurationSet()
		{
		}
	}
}