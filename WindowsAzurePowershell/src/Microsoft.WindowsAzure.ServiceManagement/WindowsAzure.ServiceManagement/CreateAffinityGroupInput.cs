using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="CreateAffinityGroup", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CreateAffinityGroupInput : IExtensibleDataObject
	{
		[DataMember(Order=2, Name="Label")]
		private string _base64EncodedLabel
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
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

		[DataMember(Order=1)]
		public string Name
		{
			get;
			set;
		}

		public CreateAffinityGroupInput()
		{
		}
	}
}