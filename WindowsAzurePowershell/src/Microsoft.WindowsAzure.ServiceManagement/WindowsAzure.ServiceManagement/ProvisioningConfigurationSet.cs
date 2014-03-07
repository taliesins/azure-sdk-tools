using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public abstract class ProvisioningConfigurationSet : ConfigurationSet
	{
		protected ProvisioningConfigurationSet()
		{
		}
	}
}