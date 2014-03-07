using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Name="Rule", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AccessControlListRule : Mergable<AccessControlListRule>
	{
		private const string OrderMemberName = "Order";

		private const string ActionMemberName = "Action";

		private const string RemoteSubnetMemberName = "RemoteSubnet";

		private const string DescriptionMemberName = "Description";

		[DataMember(Name="Action", IsRequired=false, Order=1)]
		public string Action
		{
			get
			{
				return base.GetValue<string>("Action");
			}
			set
			{
				base.SetValue<string>("Action", value);
			}
		}

		[DataMember(Name="Description", IsRequired=false, Order=3)]
		public string Description
		{
			get
			{
				return base.GetValue<string>("Description");
			}
			set
			{
				base.SetValue<string>("Description", value);
			}
		}

		[DataMember(Name="Order", IsRequired=false, Order=0)]
		public int? Order
		{
			get
			{
				return base.GetValue<int?>("Order");
			}
			set
			{
				base.SetValue<int?>("Order", value);
			}
		}

		[DataMember(Name="RemoteSubnet", IsRequired=false, Order=2)]
		public string RemoteSubnet
		{
			get
			{
				return base.GetValue<string>("RemoteSubnet");
			}
			set
			{
				base.SetValue<string>("RemoteSubnet", value);
			}
		}

		public AccessControlListRule()
		{
		}
	}
}