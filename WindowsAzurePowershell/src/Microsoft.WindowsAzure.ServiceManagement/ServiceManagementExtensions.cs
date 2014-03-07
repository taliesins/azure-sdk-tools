using System;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public static class ServiceManagementExtensions
	{
		[SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
		public static IContextChannel ToContextChannel(this IServiceManagement client)
		{
			return ServiceManagementChannelProxy.GetInterface<IContextChannel>(client);
		}
	}
}