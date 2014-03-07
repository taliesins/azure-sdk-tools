using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Connection : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Type
		{
			get;
			set;
		}

		public Connection()
		{
		}
	}
}