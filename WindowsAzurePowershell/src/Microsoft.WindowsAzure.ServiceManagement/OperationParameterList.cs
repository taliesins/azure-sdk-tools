using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="OperationParameters", ItemName="OperationParameter")]
	public class OperationParameterList : List<OperationParameter>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public OperationParameterList()
		{
		}

		public OperationParameterList(IEnumerable<OperationParameter> operations) : base(operations)
		{
		}
	}
}