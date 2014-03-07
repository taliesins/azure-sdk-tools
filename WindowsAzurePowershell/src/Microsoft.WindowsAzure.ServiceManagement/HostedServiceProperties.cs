using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class HostedServiceProperties : IExtensibleDataObject
	{
		[DataMember(Order=4, Name="Label")]
		private string _base64EncodedLabel
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6, Name="DateCreated")]
		private string _isoDateCreatedString
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=7, Name="DateLastModified")]
		private string _isoDateLastModifiedString
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

		public DateTime DateCreated
		{
			get
			{
				if (!string.IsNullOrEmpty(this._isoDateCreatedString))
				{
					return DateTime.Parse(this._isoDateCreatedString);
				}
				return new DateTime();
			}
			set
			{
				this._isoDateCreatedString = value.ToString(Constants.StandardTimeFormat);
			}
		}

		public DateTime DateLastModified
		{
			get
			{
				if (!string.IsNullOrEmpty(this._isoDateLastModifiedString))
				{
					return DateTime.Parse(this._isoDateLastModifiedString);
				}
				return new DateTime();
			}
			set
			{
				this._isoDateLastModifiedString = value.ToString(Constants.StandardTimeFormat);
			}
		}

		[DataMember(Order=1)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=8)]
		public ExtendedPropertiesList ExtendedProperties
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=9, EmitDefaultValue=false)]
		public string GuestAgentType
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

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string Status
		{
			get;
			set;
		}

		public HostedServiceProperties()
		{
		}
	}
}