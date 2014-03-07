using System;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public interface IMergable
	{
		void Merge(object other);
	}
}