using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static System.Resources.ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static string AccessDisposedClientError
		{
			get
			{
				return Resources.ResourceManager.GetString("AccessDisposedClientError", Resources.resourceCulture);
			}
		}

		internal static string ArgumentNullError
		{
			get
			{
				return Resources.ResourceManager.GetString("ArgumentNullError", Resources.resourceCulture);
			}
		}

		internal static string BehaviorIsNotMessageInspector
		{
			get
			{
				return Resources.ResourceManager.GetString("BehaviorIsNotMessageInspector", Resources.resourceCulture);
			}
		}

		internal static string CouldNotSetRequestHeaderError
		{
			get
			{
				return Resources.ResourceManager.GetString("CouldNotSetRequestHeaderError", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static string InitDisposedClientError
		{
			get
			{
				return Resources.ResourceManager.GetString("InitDisposedClientError", Resources.resourceCulture);
			}
		}

		internal static string IntArgumentLessThanZeroError
		{
			get
			{
				return Resources.ResourceManager.GetString("IntArgumentLessThanZeroError", Resources.resourceCulture);
			}
		}

		internal static string None
		{
			get
			{
				return Resources.ResourceManager.GetString("None", Resources.resourceCulture);
			}
		}

		internal static string NoOperationIdAsync
		{
			get
			{
				return Resources.ResourceManager.GetString("NoOperationIdAsync", Resources.resourceCulture);
			}
		}

		internal static string NoSubscriptionIdAsync
		{
			get
			{
				return Resources.ResourceManager.GetString("NoSubscriptionIdAsync", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					Resources.resourceMan = new System.Resources.ResourceManager("Microsoft.WindowsAzure.ServiceManagement.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		internal static string ServiceManagementClientExceptionStringFormat
		{
			get
			{
				return Resources.ResourceManager.GetString("ServiceManagementClientExceptionStringFormat", Resources.resourceCulture);
			}
		}

		internal static string ServiceManagementTimeoutError
		{
			get
			{
				return Resources.ResourceManager.GetString("ServiceManagementTimeoutError", Resources.resourceCulture);
			}
		}

		internal static string StringArgumentEmptyError
		{
			get
			{
				return Resources.ResourceManager.GetString("StringArgumentEmptyError", Resources.resourceCulture);
			}
		}

		internal static string UnableToDecodeBase64String
		{
			get
			{
				return Resources.ResourceManager.GetString("UnableToDecodeBase64String", Resources.resourceCulture);
			}
		}

		internal static string UnknownHttpStatusError
		{
			get
			{
				return Resources.ResourceManager.GetString("UnknownHttpStatusError", Resources.resourceCulture);
			}
		}

		internal static string UnknownOperationStatus
		{
			get
			{
				return Resources.ResourceManager.GetString("UnknownOperationStatus", Resources.resourceCulture);
			}
		}

		internal static string UnrecognizedTraceType
		{
			get
			{
				return Resources.ResourceManager.GetString("UnrecognizedTraceType", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}