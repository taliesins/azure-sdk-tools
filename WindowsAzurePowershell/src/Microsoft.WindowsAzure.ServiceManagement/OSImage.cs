using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OSImage : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Category
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=9)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=8)]
		public string Eula
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=14)]
		public Uri IconUri
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=10)]
		public string ImageFamily
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=13)]
		public bool? IsPremium
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=19)]
		public string Language
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public int LogicalSizeInGB
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public Uri MediaLink
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=7)]
		public string OS
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=15)]
		public Uri PrivacyUri
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=12)]
		public DateTime? PublishedDate
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=17)]
		public string PublisherName
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=16)]
		public string RecommendedVMSize
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=11)]
		public bool? ShowInGui
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=18)]
		public Uri SmallIconUri
		{
			get;
			set;
		}

		public OSImage()
		{
		}
	}
}