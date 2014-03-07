using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="UpdateAffinityGroup", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateAffinityGroupInput : IExtensibleDataObject
	{
		[DataMember(Order=1, Name="Label")]
		private string _base64EncodedLabel
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
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

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string LocationConstraint
		{
			get;
			set;
		}

		public UpdateAffinityGroupInput()
		{
		}
	}
}