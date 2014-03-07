using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	internal class BlobTransferTraceSource
	{
		private static BlobTransferTraceSource blobTransferTraceSource;

		private TraceSource traceSource = new TraceSource("DataMovement", SourceLevels.Off);

		public StringDictionary Attributes
		{
			get
			{
				return this.traceSource.Attributes;
			}
		}

		public TraceListenerCollection Listeners
		{
			get
			{
				return this.traceSource.Listeners;
			}
		}

		public SourceSwitch Switch
		{
			get
			{
				return this.traceSource.Switch;
			}
			set
			{
				this.traceSource.Switch = value;
			}
		}

		static BlobTransferTraceSource()
		{
			BlobTransferTraceSource.blobTransferTraceSource = new BlobTransferTraceSource();
		}

		private BlobTransferTraceSource()
		{
		}

		public void Close()
		{
			this.traceSource.Close();
		}

		public void Flush()
		{
			this.traceSource.Flush();
		}

		public static BlobTransferTraceSource GetInstance()
		{
			return BlobTransferTraceSource.blobTransferTraceSource;
		}

		[Conditional("TRACE")]
		public void TraceData(TraceEventType eventType, int id, object data)
		{
			this.TraceHelper(() => this.traceSource.TraceData(eventType, id, data));
		}

		[Conditional("TRACE")]
		public void TraceData(TraceEventType eventType, int id, params object[] data)
		{
			this.TraceHelper(() => this.traceSource.TraceData(eventType, id, data));
		}

		[Conditional("TRACE")]
		public void TraceEvent(TraceEventType eventType, int id)
		{
			this.TraceHelper(() => this.traceSource.TraceEvent(eventType, id));
		}

		[Conditional("TRACE")]
		public void TraceEvent(TraceEventType eventType, int id, string message)
		{
			this.TraceHelper(() => this.traceSource.TraceEvent(eventType, id, message));
		}

		[Conditional("TRACE")]
		public void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
		{
			this.TraceHelper(() => this.traceSource.TraceEvent(eventType, id, format, args));
		}

		private void TraceHelper(Action trace)
		{
			try
			{
				trace();
			}
			catch (Exception)
			{
			}
		}

		[Conditional("TRACE")]
		public void TraceInformation(string message)
		{
			this.TraceHelper(() => this.traceSource.TraceInformation(message));
		}

		[Conditional("TRACE")]
		public void TraceInformation(string format, params object[] args)
		{
			this.TraceHelper(() => this.traceSource.TraceInformation(format, args));
		}
	}
}