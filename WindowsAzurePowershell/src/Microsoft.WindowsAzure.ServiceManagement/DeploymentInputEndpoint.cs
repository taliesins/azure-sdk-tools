using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DeploymentInputEndpoint : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public int Port
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string RoleName
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Vip
		{
			get;
			set;
		}

		public DeploymentInputEndpoint()
		{
		}
	}
}