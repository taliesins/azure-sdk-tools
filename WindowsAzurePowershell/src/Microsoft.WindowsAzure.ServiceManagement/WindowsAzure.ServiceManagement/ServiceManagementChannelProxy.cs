using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	internal sealed class ServiceManagementChannelProxy : RealProxy
	{
		private const string ComponentTraceName = "ServiceManagemenChannelProxy";

		private IServiceManagement _sink;

		private TraceSourceHelper _logger;

		private RetryPolicy _retryPolicy;

		private TimeSpan operationWaitTimeout;

		private bool _beingCalledAsync;

		private readonly static TimeSpan PollAsyncOperationInterval;

		static ServiceManagementChannelProxy()
		{
			ServiceManagementChannelProxy.PollAsyncOperationInterval = new TimeSpan(0, 0, 30);
		}

		[EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public ServiceManagementChannelProxy(IServiceManagement sink, bool operationsWillBeCalledAsync, ServiceManagementClientOptions clientOptions) : base(typeof(IServiceManagement))
		{
			ArgumentValidator.CheckIfNull("sink", sink);
			ArgumentValidator.CheckIfNull("clientOptions", clientOptions);
			ArgumentValidator.CheckIfNull("clientOptions.RetryPolicy", clientOptions.RetryPolicy);
			this._sink = sink;
			this._logger = new TraceSourceHelper(clientOptions.Logger, clientOptions.ErrorEventId, "ServiceManagemenChannelProxy");
			this._retryPolicy = clientOptions.RetryPolicy;
			this.operationWaitTimeout = clientOptions.WaitTimeForOperationToComplete;
			this._beingCalledAsync = operationsWillBeCalledAsync;
			if (this._beingCalledAsync)
			{
				this._retryPolicy = RetryPolicy.NoRetryPolicy;
			}
		}

		[EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public static T GetInterface<T>(object proxy)
		{
			return (T)((ServiceManagementChannelProxy)RemotingServices.GetRealProxy(proxy))._sink;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
		public override IMessage Invoke(IMessage msg)
		{
			ServiceManagementClientException serviceManagementClientException;
			ReturnMessage returnMessage = null;
			MethodCallMessageWrapper methodCallMessageWrapper = new MethodCallMessageWrapper((IMethodCallMessage)msg);
			MethodInfo methodBase = (MethodInfo)methodCallMessageWrapper.MethodBase;
			uint num = 0;
			bool flag = false;
			do
			{
				flag = false;
				serviceManagementClientException = null;
				try
				{
					try
					{
						Func<ReturnMessage> func = () => {
							object[] args;
							string subscriptionId = null;
							if (methodCallMessageWrapper.Args.Count<object>() != 1 || !(methodCallMessageWrapper.Args[0] is ServiceManagementChannelProxy.ServiceManagementAsyncResult))
							{
								args = methodCallMessageWrapper.Args;
								if (methodCallMessageWrapper.Args.Count<object>() > 0)
								{
									subscriptionId = (string)methodCallMessageWrapper.Args[0];
								}
							}
							else
							{
								ServiceManagementChannelProxy.ServiceManagementAsyncResult serviceManagementAsyncResult = (ServiceManagementChannelProxy.ServiceManagementAsyncResult)methodCallMessageWrapper.Args[0];
								args = new object[] { serviceManagementAsyncResult._baseResult };
								subscriptionId = serviceManagementAsyncResult.SubscriptionId;
							}
							this._logger.LogDebugInformation("Attempting to invoke {0} method on Service Management API.", new object[] { methodBase.Name });
							object obj = methodBase.Invoke(this._sink, args);
							if (obj is IAsyncResult)
							{
								this._logger.LogDebugInformation("Returning IAsyncResult as custom ServiceManagementAsyncResult.");
								obj = new ServiceManagementChannelProxy.ServiceManagementAsyncResult((IAsyncResult)obj, subscriptionId, methodBase, args);
							}
							if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null)
							{
								HttpResponseMessageProperty item = (HttpResponseMessageProperty)OperationContext.Current.IncomingMessageProperties[HttpResponseMessageProperty.Name];
								if (item != null)
								{
									this._logger.LogDebugInformation("A response property was received by the client.");
									if (item.StatusCode == HttpStatusCode.Accepted && !this._beingCalledAsync)
									{
										this._logger.LogDebugInformation("The operation is asynchronous so the client will wait for completion.");
										string str = null;
										if (item.Headers != null)
										{
											str = item.Headers["x-ms-request-id"];
										}
										if (string.IsNullOrEmpty(str))
										{
											throw new InvalidOperationException(Resources.NoOperationIdAsync);
										}
										if (string.IsNullOrEmpty(subscriptionId))
										{
											throw new InvalidOperationException(Resources.NoSubscriptionIdAsync);
										}
										this.WaitForOperationToComplete(subscriptionId, str, this.operationWaitTimeout, ServiceManagementChannelProxy.PollAsyncOperationInterval);
									}
								}
							}
							return new ReturnMessage(obj, args, (int)args.Length, methodCallMessageWrapper.LogicalCallContext, methodCallMessageWrapper);
						};
						if (OperationContext.Current == null)
						{
							this._logger.LogDebugInformation("Invoking method using new OperationContext.");
							using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)this._sink))
							{
								returnMessage = func();
							}
						}
						else
						{
							this._logger.LogDebugInformation("Invoking method using existing OperationContext.");
							returnMessage = func();
						}
					}
					catch (TargetInvocationException targetInvocationException1)
					{
						TargetInvocationException targetInvocationException = targetInvocationException1;
						this._logger.LogDebugInformation("An error occurred calling the Service Management API.");
						if (targetInvocationException.InnerException == null)
						{
							this._logger.LogError("TargetInvocationException occurred while executing Service Management API operation: {0}.", new object[] { targetInvocationException });
							throw;
						}
						CommunicationException innerException = targetInvocationException.InnerException as CommunicationException;
						if (innerException == null)
						{
							if (!(targetInvocationException.InnerException is ServiceManagementClientException))
							{
								this._logger.LogError("Exception occurred while executing Service Management API operation: {0}.", new object[] { targetInvocationException.InnerException });
							}
							throw targetInvocationException.InnerException;
						}
						this.ThrowRestFaultFromComEx(innerException);
					}
				}
				catch (ServiceManagementClientException serviceManagementClientException2)
				{
					ServiceManagementClientException serviceManagementClientException1 = serviceManagementClientException2;
					num++;
					flag = (!this._retryPolicy.ShouldRetryErrorCode(serviceManagementClientException1.HttpStatus) ? false : (ulong)num <= (ulong)this._retryPolicy.RetryLimit);
					serviceManagementClientException = serviceManagementClientException1;
					if (flag)
					{
						TraceSourceHelper traceSourceHelper = this._logger;
						object[] totalMinutes = new object[] { this._retryPolicy.RetryWaitInterval.TotalMinutes, num, this._retryPolicy.RetryLimit };
						traceSourceHelper.LogInformation("Waiting for {0} minutes then attempting retry {1} of {2} on the failed operation.", totalMinutes);
						Thread.Sleep(this._retryPolicy.RetryWaitInterval);
						if (methodCallMessageWrapper.Args.Count<object>() == 1 && methodCallMessageWrapper.Args[0] is ServiceManagementChannelProxy.ServiceManagementAsyncResult)
						{
							ServiceManagementChannelProxy.ServiceManagementAsyncResult serviceManagementAsyncResult1 = (ServiceManagementChannelProxy.ServiceManagementAsyncResult)methodCallMessageWrapper.Args[0];
							this._logger.LogDebugInformation("Attempting to retry the Begin* method for the failed operation.");
							object obj1 = serviceManagementAsyncResult1.BeginMethod.Invoke(this._sink, serviceManagementAsyncResult1.BeginMethodArgs);
							if (!(obj1 is IAsyncResult))
							{
								TraceSourceHelper traceSourceHelper1 = this._logger;
								object[] fullName = new object[] { obj1.GetType().FullName };
								traceSourceHelper1.LogError("Retrying the operation failed because the begin operation did not return the expected result. Return type: {0}.", fullName);
								flag = false;
							}
							else
							{
								this._logger.LogDebugInformation("The retried begin operation returned an IAsyncResult as expected.");
								serviceManagementAsyncResult1._baseResult = (IAsyncResult)obj1;
							}
						}
					}
				}
			}
			while (flag);
			if (serviceManagementClientException != null)
			{
				this._logger.LogDebugInformation("The operation failed and was either not retryable or the retry limit was exceeded.");
				throw serviceManagementClientException;
			}
			return returnMessage;
		}

		private void ThrowRestFaultFromComEx(CommunicationException cex)
		{
			WebException innerException = cex.InnerException as WebException;
			if (innerException != null)
			{
				HttpWebResponse response = innerException.Response as HttpWebResponse;
				if (response != null)
				{
					ServiceManagementError serviceManagementError = null;
					string empty = string.Empty;
					if (response.Headers != null)
					{
						empty = response.Headers["x-ms-request-id"];
					}
					TraceSourceHelper traceSourceHelper = this._logger;
					object[] objArray = new object[] { (string.IsNullOrEmpty(empty) ? "<NONE>" : empty) };
					traceSourceHelper.LogDebugInformation("Operation ID of the failed operation: {0}.", objArray);
					try
					{
						Stream responseStream = response.GetResponseStream();
						if (responseStream.Length > (long)0)
						{
							try
							{
								this._logger.LogDebugInformation("Attempting to parse error details.");
								using (XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(responseStream, new XmlDictionaryReaderQuotas()))
								{
									DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ServiceManagementError));
									serviceManagementError = (ServiceManagementError)dataContractSerializer.ReadObject(xmlDictionaryReader, true);
									this._logger.LogDebugInformation("Error details processed successfully.");
								}
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
					ServiceManagementClientException serviceManagementClientException = new ServiceManagementClientException(response.StatusCode, serviceManagementError, empty, response.Headers);
					string code = "<NONE>";
					string message = "<NONE>";
					if (serviceManagementClientException.ErrorDetails != null)
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
					object[] statusCode = new object[] { (int)response.StatusCode, code, message, null };
					statusCode[3] = (string.IsNullOrEmpty(empty) ? "<NONE>" : empty);
					traceSourceHelper1.LogInformation("PROXYERROR, {0}, {1}, {2}, {3}", statusCode);
					this._logger.LogError("Error details: {0}", new object[] { serviceManagementClientException });
					throw serviceManagementClientException;
				}
				this._logger.LogDebugInformation("Could not process REST fault because WebException does not have response object.");
				throw innerException;
			}
			this._logger.LogDebugInformation("Could not process REST fault because CommunicationException does not have inner WebException.");
			throw cex;
		}

		private void WaitForOperationToComplete(string subscriptionId, string operationTrackingId, TimeSpan waitTimeout, TimeSpan pollingInterval)
		{
			ArgumentValidator.CheckIfNull("subscriptionId", subscriptionId);
			ArgumentValidator.CheckIfEmptyString("subscriptionId", subscriptionId);
			ArgumentValidator.CheckIfNull("operationTrackingId", operationTrackingId);
			ArgumentValidator.CheckIfEmptyString("operationTrackingId", operationTrackingId);
			DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.Add(waitTimeout);
			TraceSourceHelper traceSourceHelper = this._logger;
			object[] objArray = new object[] { operationTrackingId, dateTimeOffset.ToUniversalTime() };
			traceSourceHelper.LogInformation("Waiting for operation {0} to complete until {1}.", objArray);
			TraceSourceHelper traceSourceHelper1 = this._logger;
			object[] objArray1 = new object[] { waitTimeout, pollingInterval };
			traceSourceHelper1.LogDebugInformation("waitTimeout: {0}. pollingInterval: {1}", objArray1);
			Operation operation = null;
			do
			{
				this._logger.LogInformation("Getting status for operation {0}.", new object[] { operationTrackingId });
				operation = this._sink.EndGetOperationStatus(this._sink.BeginGetOperationStatus(subscriptionId, operationTrackingId, null, null));
				if (operation != null)
				{
					TraceSourceHelper traceSourceHelper2 = this._logger;
					object[] objArray2 = new object[] { operationTrackingId, operation.Status };
					traceSourceHelper2.LogInformation("Operation {0}: {1}.", objArray2);
					if (!operation.Status.Equals("InProgress", StringComparison.InvariantCultureIgnoreCase))
					{
						this._logger.LogInformation("Operation is {0} is no longer in progress.", new object[] { operationTrackingId });
						break;
					}
				}
				TraceSourceHelper traceSourceHelper3 = this._logger;
				object[] objArray3 = new object[] { operationTrackingId, pollingInterval.TotalSeconds };
				traceSourceHelper3.LogInformation("Operation {0} is still running. Waiting for an additional {1} seconds.", objArray3);
				Thread.Sleep(pollingInterval);
			}
			while (operation.Status.Equals("InProgress", StringComparison.InvariantCultureIgnoreCase) && DateTimeOffset.UtcNow < dateTimeOffset);
			if (operation == null || operation.Status.Equals("InProgress", StringComparison.InvariantCultureIgnoreCase))
			{
				TraceSourceHelper traceSourceHelper4 = this._logger;
				object[] objArray4 = new object[] { operationTrackingId, waitTimeout.TotalSeconds };
				traceSourceHelper4.LogError("Operation {0} did not complete within {1} seconds.", objArray4);
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string serviceManagementTimeoutError = Resources.ServiceManagementTimeoutError;
				object[] objArray5 = new object[] { operation.OperationTrackingId, waitTimeout.TotalSeconds };
				throw new TimeoutException(string.Format(invariantCulture, serviceManagementTimeoutError, objArray5));
			}
			if (operation.Status.Equals("Failed", StringComparison.InvariantCultureIgnoreCase))
			{
				if (Enum.IsDefined(typeof(HttpStatusCode), operation.HttpStatusCode))
				{
					HttpStatusCode obj = (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), operation.HttpStatusCode);
					if (operation.Error != null)
					{
						TraceSourceHelper traceSourceHelper5 = this._logger;
						object[] objArray6 = new object[] { operationTrackingId, obj, null, null };
						objArray6[2] = (operation.Error.Code == null ? "<NONE>" : operation.Error.Code);
						objArray6[3] = (operation.Error.Message == null ? "<NONE>" : operation.Error.Message);
						traceSourceHelper5.LogError("Operation {0} did not complete successfully. HTTP status: {1}. Service Management error code: {2}. Service Management error message: {3}.", objArray6);
					}
					else
					{
						TraceSourceHelper traceSourceHelper6 = this._logger;
						object[] objArray7 = new object[] { operationTrackingId, obj };
						traceSourceHelper6.LogError("Operation {0} did not complete successfully. HTTP status: {1}.", objArray7);
					}
					throw new ServiceManagementClientException(obj, operation.Error, operation.OperationTrackingId);
				}
				TraceSourceHelper traceSourceHelper7 = this._logger;
				object[] objArray8 = new object[] { operationTrackingId, operation.HttpStatusCode };
				traceSourceHelper7.LogError("Operation {0} returned an unrecognized HTTP status: {1}.", objArray8);
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string unknownHttpStatusError = Resources.UnknownHttpStatusError;
				object[] objArray9 = new object[] { operation.OperationTrackingId, operation.HttpStatusCode };
				throw new InvalidOperationException(string.Format(cultureInfo, unknownHttpStatusError, objArray9));
			}
			if (!operation.Status.Equals("Succeeded", StringComparison.InvariantCultureIgnoreCase))
			{
				TraceSourceHelper traceSourceHelper8 = this._logger;
				object[] objArray10 = new object[] { operationTrackingId, operation.Status };
				traceSourceHelper8.LogError("Operation {0} returned an unrecognized operation status: {1}.", objArray10);
				CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
				string unknownOperationStatus = Resources.UnknownOperationStatus;
				object[] objArray11 = new object[] { operation.OperationTrackingId, operation.Status };
				throw new InvalidOperationException(string.Format(invariantCulture1, unknownOperationStatus, objArray11));
			}
			this._logger.LogInformation("Operation {0} completed successfully.", new object[] { operationTrackingId });
		}

		private class ServiceManagementAsyncResult : IAsyncResult
		{
			internal IAsyncResult _baseResult;

			public object AsyncState
			{
				get
				{
					return this._baseResult.AsyncState;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					return this._baseResult.AsyncWaitHandle;
				}
			}

			internal MethodInfo BeginMethod
			{
				get;
				private set;
			}

			internal object[] BeginMethodArgs
			{
				get;
				private set;
			}

			public bool CompletedSynchronously
			{
				get
				{
					return this._baseResult.CompletedSynchronously;
				}
			}

			public bool IsCompleted
			{
				get
				{
					return this._baseResult.IsCompleted;
				}
			}

			public string SubscriptionId
			{
				get;
				private set;
			}

			public ServiceManagementAsyncResult(IAsyncResult baseResult, string subscriptionId, MethodInfo beginMethod, object[] beginMethodArgs)
			{
				this._baseResult = baseResult;
				this.SubscriptionId = subscriptionId;
				this.BeginMethod = beginMethod;
				this.BeginMethodArgs = beginMethodArgs;
			}
		}
	}
}