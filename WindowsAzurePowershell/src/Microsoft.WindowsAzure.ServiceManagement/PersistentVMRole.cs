using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class PersistentVMRole : Role
	{
		[DataMember(Name="AvailabilitySetName", EmitDefaultValue=false, Order=0)]
		public string AvailabilitySetName
		{
			get
			{
				return base.GetValue<string>("AvailabilitySetName");
			}
			set
			{
				base.SetValue<string>("AvailabilitySetName", value);
			}
		}

		[DataMember(Name="DataVirtualHardDisks", EmitDefaultValue=false, Order=1)]
		public Collection<DataVirtualHardDisk> DataVirtualHardDisks
		{
			get
			{
				return base.GetValue<Collection<DataVirtualHardDisk>>("DataVirtualHardDisks");
			}
			set
			{
				base.SetValue<Collection<DataVirtualHardDisk>>("DataVirtualHardDisks", value);
			}
		}

		[DataMember(Name="DefaultWinRmCertificateThumbprint", EmitDefaultValue=false, Order=5)]
		public string DefaultWinRmCertificateThumbprint
		{
			get
			{
				return base.GetValue<string>("DefaultWinRmCertificateThumbprint");
			}
			set
			{
				base.SetValue<string>("DefaultWinRmCertificateThumbprint", value);
			}
		}

		[DataMember(Name="Label", EmitDefaultValue=false, Order=2)]
		public string Label
		{
			get
			{
				return base.GetValue<string>("Label");
			}
			set
			{
				base.SetValue<string>("Label", value);
			}
		}

		[DataMember(Name="OSVirtualHardDisk", EmitDefaultValue=false, Order=3)]
		public Microsoft.WindowsAzure.ServiceManagement.OSVirtualHardDisk OSVirtualHardDisk
		{
			get
			{
				return base.GetValue<Microsoft.WindowsAzure.ServiceManagement.OSVirtualHardDisk>("OSVirtualHardDisk");
			}
			set
			{
				base.SetValue<Microsoft.WindowsAzure.ServiceManagement.OSVirtualHardDisk>("OSVirtualHardDisk", value);
			}
		}

		public override string RoleName
		{
			get
			{
				return base.GetValue<string>("RoleName");
			}
			set
			{
				base.SetValue<string>("RoleName", value);
			}
		}

		[DataMember(Name="RoleSize", EmitDefaultValue=false, Order=4)]
		public string RoleSize
		{
			get
			{
				return base.GetValue<string>("RoleSize");
			}
			set
			{
				base.SetValue<string>("RoleSize", value);
			}
		}

		public override string RoleType
		{
			get
			{
				return typeof(PersistentVMRole).Name;
			}
			set
			{
				base.RoleType = value;
			}
		}

		public PersistentVMRole()
		{
		}

		private static class PersistentVMRoleConstants
		{
			public const string RoleName = "RoleName";

			public const string AvailabilitySetName = "AvailabilitySetName";

			public const string DataVirtualHardDisks = "DataVirtualHardDisks";

			public const string Label = "Label";

			public const string OSVirtualHardDisk = "OSVirtualHardDisk";

			public const string RoleSize = "RoleSize";

			public const string DefaultWinRmCertificateThumbprint = "DefaultWinRmCertificateThumbprint";
		}
	}
}