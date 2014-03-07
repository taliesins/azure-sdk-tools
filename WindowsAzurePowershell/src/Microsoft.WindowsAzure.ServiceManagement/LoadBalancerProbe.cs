using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LoadBalancerProbe : Mergable<LoadBalancerProbe>
	{
		[IgnoreDataMember]
		private const string IntervalFieldName = "IntervalInSeconds";

		[IgnoreDataMember]
		private const string TimeoutFieldName = "TimeoutInSeconds";

		[DataMember(Name="IntervalInSeconds", EmitDefaultValue=false, Order=3)]
		public int? IntervalInSeconds
		{
			get
			{
				return base.GetValue<int?>("IntervalInSeconds");
			}
			set
			{
				base.SetValue<int?>("IntervalInSeconds", value);
			}
		}

		[DataMember(Name="Path", EmitDefaultValue=false, Order=0)]
		public string Path
		{
			get
			{
				return base.GetValue<string>("Path");
			}
			set
			{
				base.SetValue<string>("Path", value);
			}
		}

		[DataMember(Name="Port", EmitDefaultValue=false, Order=1)]
		private int? port
		{
			get
			{
				return base.GetField<int>("Port");
			}
			set
			{
				base.SetField<int>("Port", value);
			}
		}

		public int Port
		{
			get
			{
				return base.GetValue<int>("Port");
			}
			set
			{
				base.SetValue<int>("Port", value);
			}
		}

		[DataMember(Name="Protocol", EmitDefaultValue=false, Order=2)]
		public string Protocol
		{
			get
			{
				return base.GetValue<string>("Protocol");
			}
			set
			{
				base.SetValue<string>("Protocol", value);
			}
		}

		[DataMember(Name="TimeoutInSeconds", EmitDefaultValue=false, Order=4)]
		public int? TimeoutInSeconds
		{
			get
			{
				return base.GetValue<int?>("TimeoutInSeconds");
			}
			set
			{
				base.SetValue<int?>("TimeoutInSeconds", value);
			}
		}

		public LoadBalancerProbe()
		{
		}
	}
}