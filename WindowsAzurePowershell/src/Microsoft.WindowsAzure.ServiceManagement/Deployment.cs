using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Deployment : IExtensibleDataObject
	{
		[DataMember(Order=22, EmitDefaultValue=false)]
		public PersistentVMDowntimeInfo PersistentVMDowntime;

		[DataMember(Order=7, EmitDefaultValue=false, Name="Configuration")]
		private string _base64EncodedConfiguration
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false, Name="Label")]
		private string _base64EncodedLabel
		{
			get;
			set;
		}

		[DataMember(Order=18, EmitDefaultValue=false, Name="CreatedTime")]
		private string _isoCreatedTimeString
		{
			get;
			set;
		}

		[DataMember(Order=19, EmitDefaultValue=false, Name="LastModifiedTime")]
		private string _isoLastModifiedTimeString
		{
			get;
			set;
		}

		public string Configuration
		{
			get
			{
				string str;
				if (!StringEncoder.TryDecodeFromBase64String(this._base64EncodedConfiguration, out str))
				{
					throw new InvalidOperationException(Resources.UnableToDecodeBase64String);
				}
				return str;
			}
			set
			{
				this._base64EncodedConfiguration = StringEncoder.EncodeToBase64String(value);
			}
		}

		public DateTime CreatedTime
		{
			get
			{
				if (!string.IsNullOrEmpty(this._isoCreatedTimeString))
				{
					return DateTime.Parse(this._isoCreatedTimeString);
				}
				return new DateTime();
			}
			set
			{
				this._isoCreatedTimeString = value.ToString(Constants.StandardTimeFormat);
			}
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string DeploymentSlot
		{
			get;
			set;
		}

		[DataMember(Order=21, EmitDefaultValue=false)]
		public DnsSettings Dns
		{
			get;
			set;
		}

		[DataMember(Order=20, EmitDefaultValue=false)]
		public ExtendedPropertiesList ExtendedProperties
		{
			get;
			set;
		}

		[DataMember(Order=23, EmitDefaultValue=false)]
		public Microsoft.WindowsAzure.ServiceManagement.ExtensionConfiguration ExtensionConfiguration
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=14, EmitDefaultValue=false)]
		public DeploymentInputEndpointList InputEndpointList
		{
			get;
			set;
		}

		public string Label
		{
			get
			{
				string str;
				if (!StringEncoder.TryDecodeFromBase64String(this._base64EncodedLabel, out str))
				{
					throw new InvalidOperationException(Resources.UnableToDecodeBase64String);
				}
				return str;
			}
			set
			{
				this._base64EncodedLabel = value;
			}
		}

		public DateTime LastModifiedTime
		{
			get
			{
				if (!string.IsNullOrEmpty(this._isoLastModifiedTimeString))
				{
					return DateTime.Parse(this._isoLastModifiedTimeString);
				}
				return new DateTime();
			}
			set
			{
				this._isoLastModifiedTimeString = value.ToString(Constants.StandardTimeFormat);
			}
		}

		[DataMember(Order=15, EmitDefaultValue=false)]
		public bool? Locked
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string PrivateID
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
		public Microsoft.WindowsAzure.ServiceManagement.RoleInstanceList RoleInstanceList
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false)]
		public Microsoft.WindowsAzure.ServiceManagement.RoleList RoleList
		{
			get;
			set;
		}

		[DataMember(Order=16, EmitDefaultValue=false)]
		public bool? RollbackAllowed
		{
			get;
			set;
		}

		[DataMember(Order=13, EmitDefaultValue=false)]
		public string SdkVersion
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public string Status
		{
			get;
			set;
		}

		[DataMember(Order=11, EmitDefaultValue=false)]
		public int UpgradeDomainCount
		{
			get;
			set;
		}

		[DataMember(Order=10, EmitDefaultValue=false)]
		public Microsoft.WindowsAzure.ServiceManagement.UpgradeStatus UpgradeStatus
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public Uri Url
		{
			get;
			set;
		}

		[DataMember(Order=23, EmitDefaultValue=false)]
		public VirtualIPList VirtualIPs
		{
			get;
			set;
		}

		[DataMember(Order=17, EmitDefaultValue=false)]
		public string VirtualNetworkName
		{
			get;
			set;
		}

		public Deployment()
		{
		}
	}
}