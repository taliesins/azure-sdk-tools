using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	internal sealed class ClientMessageInspector : IClientMessageInspector, IEndpointBehavior
	{
		private const string VersionHeaderString = "2013-06-01";

		private const string ComponentTraceName = "ClientMessageInspector";

		private string _userAgent;

		private Func<string> _clientRequestIdGenerator = new Func<string>(ClientMessageInspector.DefaultRequestIdGenerator);

		private TraceSourceHelper _logger;

		private IEnumerable<IClientMessageInspector> _additionalMessageInspectors;

		public ClientMessageInspector(string userAgent, Func<string> clientRequestIdGenerator, TraceSource logger, int errorEventId, IEnumerable<IClientMessageInspector> additionalMessageInspectors)
		{
			if (!string.IsNullOrEmpty(userAgent))
			{
				this._userAgent = userAgent;
			}
			if (clientRequestIdGenerator != null)
			{
				this._clientRequestIdGenerator = clientRequestIdGenerator;
			}
			this._logger = new TraceSourceHelper(logger, errorEventId, "ClientMessageInspector");
			this._additionalMessageInspectors = additionalMessageInspectors;
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void AfterReceiveReply(ref Message reply, object correlationState)
		{
			this._logger.LogDebugInformation("ClientOutputMessageInspector.AfterReceiveReply has been invoked.");
			if (correlationState != null)
			{
				this._logger.LogDebugInformation("Processing reply for correlation state: {0}.", new object[] { correlationState });
			}
			if (reply != null)
			{
				HttpResponseMessageProperty item = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
				if (item != null)
				{
					string empty = string.Empty;
					if (item.Headers != null)
					{
						empty = item.Headers["x-ms-request-id"];
					}
					ServiceManagementClientException serviceManagementClientException = null;
					if (ClientMessageInspector.IsRestFault(item.StatusCode))
					{
						this._logger.LogDebugInformation("An error occurred calling the Service Management API.");
						TraceSourceHelper traceSourceHelper = this._logger;
						object[] objArray = new object[] { (string.IsNullOrEmpty(empty) ? "<NONE>" : empty) };
						traceSourceHelper.LogDebugInformation("Operation ID of the failed operation: {0}.", objArray);
						ServiceManagementError serviceManagementError = null;
						using (MessageBuffer messageBuffer = reply.CreateBufferedCopy(2147483647))
						{
							try
							{
								this._logger.LogDebugInformation("Attempting to parse error details.");
								using (XmlDictionaryReader readerAtBodyContents = messageBuffer.CreateMessage().GetReaderAtBodyContents())
								{
									try
									{
										DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ServiceManagementError));
										serviceManagementError = (ServiceManagementError)dataContractSerializer.ReadObject(readerAtBodyContents, true);
										this._logger.LogDebugInformation("Error details processed successfully.");
									}
									catch (SerializationException serializationException)
									{
										this._logger.LogError("Could not deserialize error object into ServiceManagementError: {0}", new object[] { serializationException });
									}
								}
							}
							catch (ObjectDisposedException objectDisposedException)
							{
								this._logger.LogError("Could not process body response because the stream has been closed.");
							}
							catch (InvalidOperationException invalidOperationException)
							{
								this._logger.LogError("Could not create XmlDictionaryReader: {0}.", new object[] { invalidOperationException });
							}
							serviceManagementClientException = new ServiceManagementClientException(item.StatusCode, serviceManagementError, empty, item.Headers);
							this._logger.LogError("Error details: {0}", new object[] { serviceManagementClientException });
						}
					}
					string code = "<NONE>";
					string message = "<NONE>";
					if (serviceManagementClientException != null && serviceManagementClientException.ErrorDetails != null)
					{
						if (!string.IsNullOrEmpty(serviceManagementClientException.ErrorDetails.Code))
						{
							code = serviceManagementClientException.ErrorDetails.Code;
						}
						if (!string.IsNullOrEmpty(serviceManagementClientException.ErrorDetails.Message))
						{
							message = serviceManagementClientException.ErrorDetails.Message;
						}
					}
					TraceSourceHelper traceSourceHelper1 = this._logger;
					object[] statusCode = new object[] { item.StatusCode, code, message, empty, correlationState };
					traceSourceHelper1.LogInformation("RESPONSE, {0}, {1}, {2}, {3}, {4}", statusCode);
					if (serviceManagementClientException != null)
					{
						throw serviceManagementClientException;
					}
				}
			}
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			clientRuntime.MessageInspectors.Add(this);
			if (this._additionalMessageInspectors != null)
			{
				foreach (IClientMessageInspector _additionalMessageInspector in this._additionalMessageInspectors)
				{
					clientRuntime.MessageInspectors.Add(_additionalMessageInspector);
				}
			}
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}

		public object BeforeSendRequest(ref Message request, IClientChannel channel)
		{
			HttpRequestMessageProperty item = null;
			if (request != null && request.Properties != null)
			{
				item = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
			}
			if (item == null || request.Headers == null)
			{
				throw new InvalidOperationException(Resources.CouldNotSetRequestHeaderError);
			}
			if (!string.IsNullOrEmpty(item.Headers["x-ms-version"]))
			{
				TraceSourceHelper traceSourceHelper = this._logger;
				object[] objArray = new object[] { item.Headers["x-ms-version"] };
				traceSourceHelper.LogDebugInformation("Version header of the request is {0}.", objArray);
			}
			else
			{
				this._logger.LogDebugInformation("Adding version header {0} to request.", new object[] { "2013-06-01" });
				item.Headers["x-ms-version"] = "2013-06-01";
			}
			if (!string.IsNullOrEmpty(this._userAgent))
			{
				this._logger.LogDebugInformation("Adding UserAgent header {0} to request.", new object[] { this._userAgent });
				item.Headers[HttpRequestHeader.UserAgent] = this._userAgent;
			}
			string str = null;
			if (this._clientRequestIdGenerator != null)
			{
				str = this._clientRequestIdGenerator();
				if (!string.IsNullOrEmpty(str))
				{
					this._logger.LogDebugInformation("Adding {0} header {1} to request.", new object[] { "x-ms-client-id", str });
					item.Headers["x-ms-client-id"] = str;
				}
			}
			Guid guid = Guid.NewGuid();
			TraceSourceHelper traceSourceHelper1 = this._logger;
			object[] objArray1 = new object[] { "x-ms-client-id", guid };
			traceSourceHelper1.LogDebugInformation("Setting correlation state of request to {0}.", objArray1);
			string str1 = (string.IsNullOrEmpty(str) ? "<NONE>" : str);
			string str2 = (string.IsNullOrEmpty(this._userAgent) ? "<NONE>" : this._userAgent);
			TraceSourceHelper traceSourceHelper2 = this._logger;
			object[] method = new object[] { item.Method, request.Headers.To.ToString(), str1, str2, guid };
			traceSourceHelper2.LogInformation("REQUEST, {0}, {1}, {2}, {3}, {4}", method);
			return guid;
		}

		private static string DefaultRequestIdGenerator()
		{
			return Guid.NewGuid().ToString();
		}

		private static bool IsRestFault(HttpStatusCode statusCode)
		{
			int num = (int)statusCode;
			if (num < 400)
			{
				return false;
			}
			return num < 600;
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}
	}
}