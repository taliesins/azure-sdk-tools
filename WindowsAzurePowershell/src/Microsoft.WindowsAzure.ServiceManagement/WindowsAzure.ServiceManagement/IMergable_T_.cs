using System;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public interface IMergable<T> : IMergable
	{
		void Merge(T other);
	}
}