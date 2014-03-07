using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ServiceModel.Dispatcher;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public class ServiceManagementClientOptions
	{
		private readonly static TimeSpan DefaultWaitTimeForOperationToComplete;

		public readonly static ServiceManagementClientOptions DefaultOptions;

		public Func<string> ClientRequestIdGenerator
		{
			get;
			private set;
		}

		public int ErrorEventId
		{
			get;
			private set;
		}

		public TraceSource Logger
		{
			get;
			private set;
		}

		public IEnumerable<IClientMessageInspector> MessageInspectors
		{
			get;
			private set;
		}

		public Microsoft.WindowsAzure.ServiceManagement.RetryPolicy RetryPolicy
		{
			get;
			private set;
		}

		public string UserAgentString
		{
			get;
			private set;
		}

		public TimeSpan WaitTimeForOperationToComplete
		{
			get;
			private set;
		}

		static ServiceManagementClientOptions()
		{
			ServiceManagementClientOptions.DefaultWaitTimeForOperationToComplete = new TimeSpan(0, 20, 0);
			ServiceManagementClientOptions.DefaultOptions = new ServiceManagementClientOptions();
		}

		public ServiceManagementClientOptions() : this(null, null, null, 0, Microsoft.WindowsAzure.ServiceManagement.RetryPolicy.Default, ServiceManagementClientOptions.DefaultWaitTimeForOperationToComplete, null)
		{
		}

		public ServiceManagementClientOptions(TraceSource logger) : this(null, null, logger, 0, Microsoft.WindowsAzure.ServiceManagement.RetryPolicy.Default, ServiceManagementClientOptions.DefaultWaitTimeForOperationToComplete, null)
		{
		}

		public ServiceManagementClientOptions(TraceSource logger, int errorEventId) : this(null, null, logger, errorEventId, Microsoft.WindowsAzure.ServiceManagement.RetryPolicy.Default, ServiceManagementClientOptions.DefaultWaitTimeForOperationToComplete, null)
		{
		}

		public ServiceManagementClientOptions(string userAgent, Func<string> clientRequestIdGenerator, TraceSource logger, int errorEventId, Microsoft.WindowsAzure.ServiceManagement.RetryPolicy retryPolicy, TimeSpan waitTimeForOperationToComplete) : this(userAgent, clientRequestIdGenerator, logger, errorEventId, retryPolicy, waitTimeForOperationToComplete, null)
		{
		}

		public ServiceManagementClientOptions(string userAgent, Func<string> clientRequestIdGenerator, TraceSource logger, int errorEventId, Microsoft.WindowsAzure.ServiceManagement.RetryPolicy retryPolicy, TimeSpan waitTimeForOperationToComplete, IEnumerable<IClientMessageInspector> messageInspectors)
		{
			this.UserAgentString = userAgent;
			this.ClientRequestIdGenerator = clientRequestIdGenerator;
			this.Logger = logger;
			this.ErrorEventId = errorEventId;
			this.RetryPolicy = retryPolicy;
			this.WaitTimeForOperationToComplete = waitTimeForOperationToComplete;
			this.MessageInspectors = messageInspectors;
		}
	}
}