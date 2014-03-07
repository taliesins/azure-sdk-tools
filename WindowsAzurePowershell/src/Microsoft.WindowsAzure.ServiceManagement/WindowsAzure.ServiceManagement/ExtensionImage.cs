using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ExtensionImage : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=9, Name="PrivateConfigurationSchema")]
		private string _base64EncodedPrivateConfigurationSchema
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=8, Name="PublicConfigurationSchema")]
		private string _base64EncodedPublicConfigurationSchema
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string Description
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
		public string HostingResources
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public string Label
		{
			get;
			set;
		}

		public string PrivateConfigurationSchema
		{
			get
			{
				string str;
				if (!StringEncoder.TryDecodeFromBase64String(this._base64EncodedPrivateConfigurationSchema, out str))
				{
					throw new InvalidOperationException(Resources.UnableToDecodeBase64String);
				}
				return str;
			}
			set
			{
				this._base64EncodedPrivateConfigurationSchema = StringEncoder.EncodeToBase64String(value);
			}
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string ProviderNameSpace
		{
			get;
			set;
		}

		public string PublicConfigurationSchema
		{
			get
			{
				string str;
				if (!StringEncoder.TryDecodeFromBase64String(this._base64EncodedPublicConfigurationSchema, out str))
				{
					throw new InvalidOperationException(Resources.UnableToDecodeBase64String);
				}
				return str;
			}
			set
			{
				this._base64EncodedPublicConfigurationSchema = StringEncoder.EncodeToBase64String(value);
			}
		}

		[DataMember(EmitDefaultValue=false, Order=7)]
		public string ThumbprintAlgorithm
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Type
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string Version
		{
			get;
			set;
		}

		public ExtensionImage()
		{
		}
	}
}