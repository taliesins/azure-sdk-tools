using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract]
	public class OperationParameter : IExtensibleDataObject
	{
		private readonly static ReadOnlyCollection<Type> KnownTypes;

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=0)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		private string Value
		{
			get;
			set;
		}

		static OperationParameter()
		{
			Type[] typeArray = new Type[] { typeof(CreateAffinityGroupInput), typeof(UpdateAffinityGroupInput), typeof(CertificateFile), typeof(ChangeConfigurationInput), typeof(CreateDeploymentInput), typeof(CreateHostedServiceInput), typeof(CreateStorageServiceInput), typeof(RegenerateKeys), typeof(StorageDomain), typeof(SubscriptionCertificate), typeof(SwapDeploymentInput), typeof(UpdateDeploymentStatusInput), typeof(UpdateHostedServiceInput), typeof(UpdateStorageServiceInput), typeof(UpgradeDeploymentInput), typeof(WalkUpgradeDomainInput), typeof(CaptureRoleOperation), typeof(ShutdownRoleOperation), typeof(StartRoleOperation), typeof(RestartRoleOperation), typeof(OSImage), typeof(PersistentVMRole), typeof(Deployment), typeof(DataVirtualHardDisk), typeof(OSImage), typeof(Disk), typeof(ExtendedProperty), typeof(ExtensionConfiguration), typeof(HostedServiceExtensionInput) };
			OperationParameter.KnownTypes = new ReadOnlyCollection<Type>(typeArray);
		}

		public OperationParameter()
		{
		}

		public string GetSerializedValue()
		{
			return this.Value;
		}

		public object GetValue()
		{
			object value;
			if (string.IsNullOrEmpty(this.Value))
			{
				return null;
			}
			XmlReaderSettings xmlReaderSetting = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
				XmlResolver = null,
				DtdProcessing = DtdProcessing.Prohibit,
				MaxCharactersInDocument = (long)0,
				MaxCharactersFromEntities = (long)0
			};
			XmlReaderSettings xmlReaderSetting1 = xmlReaderSetting;
			DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(object), OperationParameter.KnownTypes);
			try
			{
				using (StringReader stringReader = new StringReader(this.Value))
				{
					value = dataContractSerializer.ReadObject(XmlReader.Create(stringReader, xmlReaderSetting1));
				}
			}
			catch
			{
				value = this.Value;
			}
			return value;
		}

		public void SetValue(object value)
		{
			if (value != null)
			{
				if (value.GetType().Equals(typeof(string)))
				{
					this.Value = (string)value;
					return;
				}
				DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(object), OperationParameter.KnownTypes);
				StringBuilder stringBuilder = new StringBuilder();
				using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
				{
					dataContractSerializer.WriteObject(xmlWriter, value);
					xmlWriter.Flush();
					this.Value = stringBuilder.ToString();
				}
			}
		}
	}
}