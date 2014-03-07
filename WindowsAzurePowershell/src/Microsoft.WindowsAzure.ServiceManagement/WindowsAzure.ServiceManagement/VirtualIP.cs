using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualIP : IExtensibleDataObject
	{
		[DataMember(Order=1, EmitDefaultValue=false)]
		public string Address
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public bool? IsDnsProgrammed
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string Name
		{
			get;
			set;
		}

		public VirtualIP()
		{
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			VirtualIP virtualIP = obj as VirtualIP;
			if (virtualIP == null)
			{
				return false;
			}
			return this == virtualIP;
		}

		public override int GetHashCode()
		{
			return this.Address.GetHashCode();
		}

		public static bool operator ==(VirtualIP left, VirtualIP right)
		{
			if (object.ReferenceEquals(left, right))
			{
				return true;
			}
			if (left == null && right == null)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			if (!string.Equals(left.Address, right.Address, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(VirtualIP left, VirtualIP right)
		{
			return !(left == right);
		}
	}
}