using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class WindowsProvisioningConfigurationSet : ProvisioningConfigurationSet
	{
		[DataMember(Name="AdminPassword", EmitDefaultValue=false, Order=2)]
		public string AdminPassword
		{
			get
			{
				return base.GetValue<string>("AdminPassword");
			}
			set
			{
				base.SetValue<string>("AdminPassword", value);
			}
		}

		[DataMember(Name="AdminUsername", EmitDefaultValue=false, Order=9)]
		public string AdminUsername
		{
			get
			{
				return base.GetValue<string>("AdminUsername");
			}
			set
			{
				base.SetValue<string>("AdminUsername", value);
			}
		}

		[DataMember(Name="ComputerName", EmitDefaultValue=false, Order=1)]
		public string ComputerName
		{
			get
			{
				return base.GetValue<string>("ComputerName");
			}
			set
			{
				base.SetValue<string>("ComputerName", value);
			}
		}

		public override string ConfigurationSetType
		{
			get
			{
				return "WindowsProvisioningConfiguration";
			}
			set
			{
				base.ConfigurationSetType = value;
			}
		}

		[DataMember(Name="DomainJoin", EmitDefaultValue=false, Order=6)]
		public WindowsProvisioningConfigurationSet.DomainJoinSettings DomainJoin
		{
			get
			{
				return base.GetValue<WindowsProvisioningConfigurationSet.DomainJoinSettings>("DomainJoin");
			}
			set
			{
				base.SetValue<WindowsProvisioningConfigurationSet.DomainJoinSettings>("DomainJoin", value);
			}
		}

		[DataMember(Name="EnableAutomaticUpdates", EmitDefaultValue=false, Order=4)]
		public bool? EnableAutomaticUpdates
		{
			get
			{
				return base.GetValue<bool?>("EnableAutomaticUpdates");
			}
			set
			{
				base.SetValue<bool?>("EnableAutomaticUpdates", value);
			}
		}

		[DataMember(Name="ResetPasswordOnFirstLogon", EmitDefaultValue=false, Order=4)]
		private bool? resetPasswordOnFirstLogon
		{
			get
			{
				return base.GetField<bool>("ResetPasswordOnFirstLogon");
			}
			set
			{
				base.SetField<bool>("ResetPasswordOnFirstLogon", value);
			}
		}

		public bool ResetPasswordOnFirstLogon
		{
			get
			{
				return base.GetValue<bool>("ResetPasswordOnFirstLogon");
			}
			set
			{
				base.SetValue<bool>("ResetPasswordOnFirstLogon", value);
			}
		}

		[DataMember(Name="StoredCertificateSettings", EmitDefaultValue=false, Order=7)]
		public CertificateSettingList StoredCertificateSettings
		{
			get
			{
				return base.GetValue<CertificateSettingList>("StoredCertificateSettings");
			}
			set
			{
				base.SetValue<CertificateSettingList>("StoredCertificateSettings", value);
			}
		}

		[DataMember(Name="TimeZone", EmitDefaultValue=false, Order=5)]
		public string TimeZone
		{
			get
			{
				return base.GetValue<string>("TimeZone");
			}
			set
			{
				base.SetValue<string>("TimeZone", value);
			}
		}

		[DataMember(Name="WinRM", EmitDefaultValue=false, Order=8)]
		public WindowsProvisioningConfigurationSet.WinRmConfiguration WinRM
		{
			get
			{
				return base.GetValue<WindowsProvisioningConfigurationSet.WinRmConfiguration>("WinRM");
			}
			set
			{
				base.SetValue<WindowsProvisioningConfigurationSet.WinRmConfiguration>("WinRM", value);
			}
		}

		public WindowsProvisioningConfigurationSet()
		{
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class DomainJoinCredentials
		{
			[DataMember(Name="Domain", EmitDefaultValue=false, Order=1)]
			public string Domain
			{
				get;
				set;
			}

			[DataMember(Name="Password", EmitDefaultValue=false, Order=3)]
			public string Password
			{
				get;
				set;
			}

			[DataMember(Name="Username", EmitDefaultValue=false, Order=2)]
			public string Username
			{
				get;
				set;
			}

			public DomainJoinCredentials()
			{
			}
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class DomainJoinProvisioning
		{
			[DataMember(Name="AccountData", EmitDefaultValue=false, Order=1)]
			public string AccountData
			{
				get;
				set;
			}

			public DomainJoinProvisioning()
			{
			}
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class DomainJoinSettings
		{
			[DataMember(Name="Credentials", EmitDefaultValue=false, Order=1)]
			public WindowsProvisioningConfigurationSet.DomainJoinCredentials Credentials
			{
				get;
				set;
			}

			[DataMember(Name="JoinDomain", EmitDefaultValue=false, Order=3)]
			public string JoinDomain
			{
				get;
				set;
			}

			[DataMember(Name="MachineObjectOU", EmitDefaultValue=false, Order=4)]
			public string MachineObjectOU
			{
				get;
				set;
			}

			[DataMember(Name="Provisioning", EmitDefaultValue=false, Order=2)]
			public WindowsProvisioningConfigurationSet.DomainJoinProvisioning Provisioning
			{
				get;
				set;
			}

			public DomainJoinSettings()
			{
			}
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class WinRmConfiguration
		{
			[DataMember(EmitDefaultValue=false, Order=1)]
			public WindowsProvisioningConfigurationSet.WinRmListenerCollection Listeners
			{
				get;
				set;
			}

			public WinRmConfiguration()
			{
			}
		}

		[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", ItemName="Listener")]
		public class WinRmListenerCollection : Collection<WindowsProvisioningConfigurationSet.WinRmListenerProperties>
		{
			public WinRmListenerCollection()
			{
			}
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class WinRmListenerProperties
		{
			[DataMember(Order=0, IsRequired=false, EmitDefaultValue=false)]
			public string CertificateThumbprint
			{
				get;
				set;
			}

			[DataMember(Order=1, IsRequired=true)]
			public string Protocol
			{
				get;
				set;
			}

			public WinRmListenerProperties()
			{
			}
		}

		public enum WinRmProtocol
		{
			Http,
			Https
		}
	}
}