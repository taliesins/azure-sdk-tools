using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystem : IExtensibleDataObject
	{
		[DataMember(Order=6, EmitDefaultValue=false, Name="FamilyLabel")]
		private string _base64EncodedFamilyLabel
		{
			get;
			set;
		}

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

		[DataMember(Order=5, EmitDefaultValue=false)]
		public string Family
		{
			get;
			set;
		}

		public string FamilyLabel
		{
			get
			{
				string str;
				if (!StringEncoder.TryDecodeFromBase64String(this._base64EncodedFamilyLabel, out str))
				{
					throw new InvalidOperationException(Resources.UnableToDecodeBase64String);
				}
				return str;
			}
			set
			{
				this._base64EncodedFamilyLabel = StringEncoder.EncodeToBase64String(value);
			}
		}

		[DataMember(Order=4)]
		public bool IsActive
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public bool IsDefault
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
		public string Version
		{
			get;
			set;
		}

		public OperatingSystem()
		{
		}
	}
}