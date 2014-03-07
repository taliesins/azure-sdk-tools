using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="ChangeConfiguration", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ChangeConfigurationInput : IExtensibleDataObject
	{
		[DataMember(Order=1, Name="Configuration")]
		private string _base64EncodedConfiguration
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

		[DataMember(Order=4, EmitDefaultValue=false)]
		public ExtendedPropertiesList ExtendedProperties
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
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

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string Mode
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public bool? TreatWarningsAsError
		{
			get;
			set;
		}

		public ChangeConfigurationInput()
		{
		}
	}
}