using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StorageServiceProperties : IExtensibleDataObject
	{
		[DataMember(Order=4, Name="Label", EmitDefaultValue=false)]
		private string _base64EncodedLabel
		{
			get;
			set;
		}

		[DataMember(Order=13, EmitDefaultValue=false, Name="CreationTime")]
		private string _isoCreationTimeString
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false, Name="LastGeoFailoverTime")]
		private string _isoLastGeoFailoverTime
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		public DateTime CreationTime
		{
			get
			{
				if (!string.IsNullOrEmpty(this._isoCreationTimeString))
				{
					return DateTime.Parse(this._isoCreationTimeString);
				}
				return new DateTime();
			}
			set
			{
				this._isoCreationTimeString = value.ToString(Constants.StandardTimeFormat);
			}
		}

		[DataMember(Order=14, EmitDefaultValue=false)]
		public CustomDomainList CustomDomains
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public EndpointList Endpoints
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
		public string GeoPrimaryRegion
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
		public bool? GeoReplicationEnabled
		{
			get;
			set;
		}

		[DataMember(Order=10, EmitDefaultValue=false)]
		public string GeoSecondaryRegion
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
				this._base64EncodedLabel = StringEncoder.EncodeToBase64String(value);
			}
		}

		public DateTime LastGeoFailoverTime
		{
			get
			{
				if (!string.IsNullOrEmpty(this._isoLastGeoFailoverTime))
				{
					return DateTime.Parse(this._isoLastGeoFailoverTime);
				}
				return new DateTime();
			}
			set
			{
				this._isoLastGeoFailoverTime = value.ToString(Constants.StandardTimeFormat);
			}
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public string Status
		{
			get;
			set;
		}

		[DataMember(Order=9, EmitDefaultValue=false)]
		public string StatusOfPrimary
		{
			get;
			set;
		}

		[DataMember(Order=11, EmitDefaultValue=false)]
		public string StatusOfSecondary
		{
			get;
			set;
		}

		public StorageServiceProperties()
		{
		}
	}
}