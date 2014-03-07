using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="CreateDeployment", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CreateDeploymentInput : IExtensibleDataObject
	{
		[DataMember(Order=4, Name="Configuration")]
		private string _base64EncodedConfiguration
		{
			get;
			set;
		}

		[DataMember(Order=3, Name="Label")]
		private string _base64EncodedLabel
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

		[DataMember(Order=7, EmitDefaultValue=false)]
		public ExtendedPropertiesList ExtendedProperties
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
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

		[DataMember(Order=1)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public Uri PackageUrl
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public bool? StartDeployment
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public bool? TreatWarningsAsError
		{
			get;
			set;
		}

		public CreateDeploymentInput()
		{
		}
	}
}