using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="SubscriptionCertificates", ItemName="SubscriptionCertificate", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubscriptionCertificateList : List<SubscriptionCertificate>
	{
		public SubscriptionCertificateList()
		{
		}

		public SubscriptionCertificateList(IEnumerable<SubscriptionCertificate> Certificates) : base(Certificates)
		{
		}
	}
}