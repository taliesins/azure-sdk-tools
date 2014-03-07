using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="Extension")]
	public class HostedServiceExtension : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=7, Name="PublicConfiguration")]
		private string _base64EncodedPublicConfiguration
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string Id
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string ProviderNameSpace
		{
			get;
			set;
		}

		public string PublicConfiguration
		{
			get
			{
				string str;
				if (!StringEncoder.TryDecodeFromBase64String(this._base64EncodedPublicConfiguration, out str))
				{
					throw new InvalidOperationException(Resources.UnableToDecodeBase64String);
				}
				return str;
			}
			set
			{
				this._base64EncodedPublicConfiguration = StringEncoder.EncodeToBase64String(value);
			}
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string Thumbprint
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
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

		[DataMember(EmitDefaultValue=false, Order=4)]
		public string Version
		{
			get;
			set;
		}

		public HostedServiceExtension()
		{
		}
	}
}