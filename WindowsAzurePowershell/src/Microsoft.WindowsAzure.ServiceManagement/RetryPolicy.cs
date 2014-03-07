using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public sealed class RetryPolicy
	{
		private const int DefaultRetryWaitIntervalMinutes = 2;

		private const int DefaultRetryLimit = 5;

		public readonly static RetryPolicy Default;

		public readonly static RetryPolicy NoRetryPolicy;

		private Func<HttpStatusCode, bool> _shouldRetry
		{
			get;
			set;
		}

		public int RetryLimit
		{
			get;
			set;
		}

		public TimeSpan RetryWaitInterval
		{
			get;
			set;
		}

		public Func<HttpStatusCode, bool> ShouldRetryErrorCode
		{
			get
			{
				return this._shouldRetry;
			}
			set
			{
				ArgumentValidator.CheckIfNull("value", value);
				this._shouldRetry = value;
			}
		}

		static RetryPolicy()
		{
			RetryPolicy.Default = new RetryPolicy();
			TimeSpan timeSpan = new TimeSpan();
			RetryPolicy.NoRetryPolicy = new RetryPolicy(timeSpan, 0, (HttpStatusCode status) => false);
		}

		public RetryPolicy()
		{
			this.RetryLimit = 5;
			this.RetryWaitInterval = TimeSpan.FromMinutes(2);
			this.ShouldRetryErrorCode = new Func<HttpStatusCode, bool>(RetryPolicy.DefaultShouldRetryFunction);
		}

		public RetryPolicy(TimeSpan retryWaitInterval, int retryLimit, Func<HttpStatusCode, bool> shouldRetry)
		{
			ArgumentValidator.CheckIfNull("shouldRetry", shouldRetry);
			ArgumentValidator.CheckIfIntLessThanZero("retryLimit", retryLimit);
			this.RetryLimit = retryLimit;
			this.RetryWaitInterval = retryWaitInterval;
			this._shouldRetry = shouldRetry;
		}

		private static bool DefaultShouldRetryFunction(HttpStatusCode httpStatus)
		{
			bool flag = false;
			HttpStatusCode httpStatusCode = httpStatus;
			if (httpStatusCode != HttpStatusCode.RequestTimeout)
			{
				switch (httpStatusCode)
				{
					case HttpStatusCode.InternalServerError:
					case HttpStatusCode.BadGateway:
					case HttpStatusCode.ServiceUnavailable:
					case HttpStatusCode.GatewayTimeout:
					{
						break;
					}
					case HttpStatusCode.NotImplemented:
					{
						flag = false;
						return flag;
					}
					default:
					{
						flag = false;
						return flag;
					}
				}
			}
			flag = true;
			return flag;
		}
	}
}