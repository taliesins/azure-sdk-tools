using System;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[Serializable]
	public class ServiceManagementClientException : Exception
	{
		public ServiceManagementError ErrorDetails
		{
			get;
			private set;
		}

		public HttpStatusCode HttpStatus
		{
			get;
			private set;
		}

		public string OperationTrackingId
		{
			get;
			private set;
		}

		public WebHeaderCollection ResponseHeaders
		{
			get;
			private set;
		}

		public ServiceManagementClientException(HttpStatusCode httpStatus, ServiceManagementError errorDetails, string operationTrackingId) : this(httpStatus, errorDetails, operationTrackingId, null)
		{
		}

		public ServiceManagementClientException(HttpStatusCode httpStatus, ServiceManagementError errorDetails, string operationTrackingId, WebHeaderCollection responseHeaders)
            : base(GetMessage(httpStatus, errorDetails, operationTrackingId))
		{
			this.HttpStatus = httpStatus;
			this.ErrorDetails = errorDetails;
			this.OperationTrackingId = operationTrackingId;
			this.ResponseHeaders = responseHeaders;
		}

        private static string GetMessage(HttpStatusCode httpStatus, ServiceManagementError errorDetails, string operationTrackingId)
	    {
	        CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string serviceManagementClientExceptionStringFormat = Resources.ServiceManagementClientExceptionStringFormat;
			object[] objArray = new object[] { (int)httpStatus, null, null, null };
			objArray[1] = (errorDetails == null || string.IsNullOrEmpty(errorDetails.Code) ? Resources.None : errorDetails.Code);
			objArray[2] = (errorDetails == null || string.IsNullOrEmpty(errorDetails.Message) ? Resources.None : errorDetails.Message);
			objArray[3] = (string.IsNullOrEmpty(operationTrackingId) ? Resources.None : operationTrackingId);
			
            return string.Format(currentCulture, serviceManagementClientExceptionStringFormat, objArray);
	    }

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}