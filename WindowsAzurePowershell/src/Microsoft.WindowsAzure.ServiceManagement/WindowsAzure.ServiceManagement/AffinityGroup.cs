using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AffinityGroup : IExtensibleDataObject
	{
		[DataMember(Order=2, Name="Label")]
		private string _base64EncodedLabel
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
		public CapabilitiesList Capabilities
		{
			get;
			set;
		}

		[DataMember(Order=3)]
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

		[DataMember(Order=5, EmitDefaultValue=false)]
		public HostedServiceList HostedServices
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

		[DataMember(Order=4)]
		public string Location
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

		[DataMember(Order=6, EmitDefaultValue=false)]
		public StorageServiceList StorageServices
		{
			get;
			set;
		}

		public AffinityGroup()
		{
		}
	}
}