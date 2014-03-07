using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystemFamily : IExtensibleDataObject
	{
		[DataMember(Order=2, EmitDefaultValue=false, Name="Label")]
		private string _base64EncodedLabel
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

		[DataMember(Order=3)]
		public OperatingSystemList OperatingSystems
		{
			get;
			set;
		}

		public OperatingSystemFamily()
		{
		}
	}
}