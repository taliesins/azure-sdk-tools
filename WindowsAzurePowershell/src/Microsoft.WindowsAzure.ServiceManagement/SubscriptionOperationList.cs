using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="SubscriptionOperations", ItemName="SubscriptionOperation")]
	public class SubscriptionOperationList : List<SubscriptionOperation>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public SubscriptionOperationList()
		{
		}

		public SubscriptionOperationList(IEnumerable<SubscriptionOperation> subscriptions) : base(subscriptions)
		{
		}
	}
}