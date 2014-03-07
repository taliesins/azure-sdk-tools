using System;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public static class Constants
	{
		public const string ContinuationTokenHeaderName = "x-ms-continuation-token";

		public const string ClientRequestIdHeader = "x-ms-client-id";

		public const string OperationTrackingIdHeader = "x-ms-request-id";

		public const string PrincipalHeader = "x-ms-principal-id";

		public const string ServiceManagementNS = "http://schemas.microsoft.com/windowsazure";

		public const string VersionHeaderName = "x-ms-version";

		public const string VersionHeaderContent20130601 = "2013-06-01";

		public const string VersionHeaderContentLatest = "2013-06-01";

		public readonly static string StandardTimeFormat;

		static Constants()
		{
			Constants.StandardTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
		}
	}
}