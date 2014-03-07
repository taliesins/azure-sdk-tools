using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	internal class TraceSourceHelper
	{
		private TraceSource _debugTrace = new TraceSource("ServiceManagementInternalDebug", SourceLevels.Verbose);

		private TraceSource _logger;

		private int _errorEventId;

		private string _componentNameString;

		public TraceSourceHelper(TraceSource logger, int errorEventId, string componentNameString)
		{
			this._logger = logger;
			this._errorEventId = errorEventId;
			this._componentNameString = componentNameString;
		}

		private string GenerateLogStamp()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] str = new object[] { DateTimeOffset.UtcNow.ToString(), this._componentNameString };
			return string.Format(invariantCulture, "{0} - {1}:", str);
		}

		public void LogDebugInformation(string format, params object[] args)
		{
			if (this._debugTrace != null)
			{
				this._debugTrace.TraceInformation(string.Concat(this.GenerateLogStamp(), format), args);
			}
		}

		public void LogDebugInformation(string msg)
		{
			if (this._debugTrace != null)
			{
				this._debugTrace.TraceInformation(string.Concat(this.GenerateLogStamp(), msg));
			}
		}

		public void LogError(string msg)
		{
			this.TraceInternal(TraceEventType.Error, msg);
		}

		public void LogError(string format, params object[] args)
		{
			this.TraceInternal(TraceEventType.Error, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public void LogInformation(string msg)
		{
			this.TraceInternal(TraceEventType.Information, msg);
		}

		public void LogInformation(string format, params object[] args)
		{
			this.TraceInternal(TraceEventType.Information, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		private void TraceInternal(TraceEventType eventKind, string msg)
		{
			if (this._logger != null)
			{
				TraceEventType traceEventType = eventKind;
				if (traceEventType == TraceEventType.Error)
				{
					TraceSource traceSource = this._logger;
					int num = this._errorEventId;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { this.GenerateLogStamp(), msg };
					traceSource.TraceEvent(TraceEventType.Error, num, string.Format(invariantCulture, "{0} {1}", objArray));
				}
				else
				{
					if (traceEventType != TraceEventType.Information)
					{
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						string unrecognizedTraceType = Resources.UnrecognizedTraceType;
						object[] objArray1 = new object[] { eventKind };
						throw new ArgumentException(string.Format(cultureInfo, unrecognizedTraceType, objArray1), "eventKind");
					}
					TraceSource traceSource1 = this._logger;
					CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
					object[] objArray2 = new object[] { this.GenerateLogStamp(), msg };
					traceSource1.TraceInformation(string.Format(invariantCulture1, "{0} {1}", objArray2));
				}
			}
			this.LogDebugInformation(msg);
		}
	}
}