using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class EndpointAccessControlList : Mergable<EndpointAccessControlList>
	{
		[IgnoreDataMember]
		private const string AccessControlListRulesMemberName = "AccessControlListRules";

		[DataMember(Name="Rules", IsRequired=false, Order=0)]
		public Collection<AccessControlListRule> Rules
		{
			get
			{
				return base.GetValue<Collection<AccessControlListRule>>("AccessControlListRules");
			}
			set
			{
				base.SetValue<Collection<AccessControlListRule>>("AccessControlListRules", value);
			}
		}

		public EndpointAccessControlList()
		{
		}
	}
}