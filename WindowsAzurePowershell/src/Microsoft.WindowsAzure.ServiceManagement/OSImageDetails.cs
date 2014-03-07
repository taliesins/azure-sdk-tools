using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OSImageDetails : OSImage
	{
		[DataMember(EmitDefaultValue=false, Order=90)]
		public bool IsCorrupted
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=100)]
		public ReplicationProgressList ReplicationProgress
		{
			get;
			set;
		}

		public OSImageDetails()
		{
		}
	}
}