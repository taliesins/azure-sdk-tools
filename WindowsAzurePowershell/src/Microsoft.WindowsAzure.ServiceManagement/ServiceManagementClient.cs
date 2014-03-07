using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Remoting.Proxies;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using System.Xml;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public class ServiceManagementClient : IDisposable
	{
		private const string ComponentTraceName = "ServiceManagementClient";

		public readonly static Uri ServiceManagementUri;

		private bool _disposed;

		private IServiceManagement _asyncService;

		private IServiceManagement _syncService;

		private string _subscriptionId;

		private X509Certificate2 _clientCert;

		private TraceSourceHelper _logger;

		public IServiceManagement Service
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException(Resources.AccessDisposedClientError);
				}
				return this._asyncService;
			}
		}

		private IServiceManagement SyncService
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException(Resources.AccessDisposedClientError);
				}
				return this._syncService;
			}
		}

		static ServiceManagementClient()
		{
			ServiceManagementClient.ServiceManagementUri = new Uri("https://management.core.windows.net");
		}

		[SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
		public ServiceManagementClient(Uri remoteUri, string subscriptionId, X509Certificate2 clientCert, ServiceManagementClientOptions clientOptions)
		{
			ArgumentValidator.CheckIfNull("remoteUri", remoteUri);
			ArgumentValidator.CheckIfNull("subscriptionId", subscriptionId);
			ArgumentValidator.CheckIfEmptyString("subscriptionId", subscriptionId);
			ArgumentValidator.CheckIfNull("clientCert", clientCert);
			ArgumentValidator.CheckIfNull("clientOptions", clientOptions);
			this._subscriptionId = subscriptionId;
			this._clientCert = clientCert;
			this.Initialize(remoteUri, clientOptions);
		}

		public void AddCertificates(string serviceName, CertificateFile input)
		{
			this.SyncService.EndAddCertificates(this.SyncService.BeginAddCertificates(this._subscriptionId, serviceName, input, null, null));
		}

		public void AddDataDisk(string serviceName, string deploymentName, string roleName, DataVirtualHardDisk dataDisk)
		{
			this.SyncService.EndAddDataDisk(this.SyncService.BeginAddDataDisk(this._subscriptionId, serviceName, deploymentName, roleName, dataDisk, null, null));
		}

		public void AddHostedServiceExtension(string serviceName, HostedServiceExtensionInput extension)
		{
			this.SyncService.EndAddHostedServiceExtension(this.SyncService.BeginAddHostedServiceExtension(this._subscriptionId, serviceName, extension, null, null));
		}

		public void AddRole(string serviceName, string deploymentName, PersistentVMRole role)
		{
			this.SyncService.EndAddRole(this.SyncService.BeginAddRole(this._subscriptionId, serviceName, deploymentName, role, null, null));
		}

		public void AddSubscriptionCertificate(SubscriptionCertificate certificate)
		{
			this.SyncService.EndAddSubscriptionCertificate(this.SyncService.BeginAddSubscriptionCertificate(this._subscriptionId, certificate, null, null));
		}

		public void CaptureRole(string serviceName, string deploymentName, string roleInstanceName, string targetImageName, string targetImageLabel, PostCaptureAction postCaptureAction, ProvisioningConfigurationSet provisioningConfiguration)
		{
			IServiceManagement syncService = this.SyncService;
			IServiceManagement serviceManagement = this.SyncService;
			string str = this._subscriptionId;
			CaptureRoleOperation captureRoleOperation = new CaptureRoleOperation()
			{
				PostCaptureAction = postCaptureAction.ToString(),
				ProvisioningConfiguration = provisioningConfiguration,
				TargetImageName = targetImageName,
				TargetImageLabel = targetImageLabel
			};
			syncService.EndExecuteRoleOperation(serviceManagement.BeginExecuteRoleOperation(str, serviceName, deploymentName, roleInstanceName, captureRoleOperation, null, null));
		}

		public void ChangeConfiguration(string serviceName, string deploymentName, ChangeConfigurationInput input)
		{
			this.SyncService.EndChangeConfiguration(this.SyncService.BeginChangeConfiguration(this._subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public void ChangeConfigurationBySlot(string serviceName, string deploymentSlot, ChangeConfigurationInput input)
		{
			this.SyncService.EndChangeConfigurationBySlot(this.SyncService.BeginChangeConfigurationBySlot(this._subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public void CreateAffinityGroup(CreateAffinityGroupInput input)
		{
			this.SyncService.EndCreateAffinityGroup(this.SyncService.BeginCreateAffinityGroup(this._subscriptionId, input, null, null));
		}

		public void CreateDeployment(string serviceName, Deployment deployment)
		{
			this.SyncService.EndCreateDeployment(this.SyncService.BeginCreateDeployment(this._subscriptionId, serviceName, deployment, null, null));
		}

		public Disk CreateDisk(Disk disk)
		{
			return this.SyncService.EndCreateDisk(this.SyncService.BeginCreateDisk(this._subscriptionId, disk, null, null));
		}

		public void CreateHostedService(CreateHostedServiceInput input)
		{
			this.SyncService.EndCreateHostedService(this.SyncService.BeginCreateHostedService(this._subscriptionId, input, null, null));
		}

		public void CreateOrUpdateDeployment(string serviceName, string deploymentSlot, CreateDeploymentInput input)
		{
			this.SyncService.EndCreateOrUpdateDeployment(this.SyncService.BeginCreateOrUpdateDeployment(this._subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public OSImage CreateOSImage(OSImage image)
		{
			return this.SyncService.EndCreateOSImage(this.SyncService.BeginCreateOSImage(this._subscriptionId, image, null, null));
		}

		public void CreateStorageService(CreateStorageServiceInput input)
		{
			this.SyncService.EndCreateStorageService(this.SyncService.BeginCreateStorageService(this._subscriptionId, input, null, null));
		}

		public void DeleteAffinityGroup(string affinityGroupName)
		{
			this.SyncService.EndDeleteAffinityGroup(this.SyncService.BeginDeleteAffinityGroup(this._subscriptionId, affinityGroupName, null, null));
		}

		public void DeleteCertificate(string serviceName, string algorithm, string thumbprint)
		{
			this.SyncService.EndDeleteCertificate(this.SyncService.BeginDeleteCertificate(this._subscriptionId, serviceName, algorithm, thumbprint, null, null));
		}

		public void DeleteDataDisk(string serviceName, string deploymentName, string roleName, int lun)
		{
			this.SyncService.EndDeleteDataDisk(this.SyncService.BeginDeleteDataDisk(this._subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), null, null));
		}

		public void DeleteDeployment(string serviceName, string deploymentName)
		{
			this.SyncService.EndDeleteDeployment(this.SyncService.BeginDeleteDeployment(this._subscriptionId, serviceName, deploymentName, null, null));
		}

		public void DeleteDeploymentBySlot(string serviceName, string deploymentSlot)
		{
			this.SyncService.EndDeleteDeploymentBySlot(this.SyncService.BeginDeleteDeploymentBySlot(this._subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public void DeleteDisk(string diskName)
		{
			this.SyncService.EndDeleteDisk(this.SyncService.BeginDeleteDisk(this._subscriptionId, diskName, null, null));
		}

		public void DeleteDiskEx(string diskName, string comp)
		{
			this.SyncService.EndDeleteDiskEx(this.SyncService.BeginDeleteDiskEx(this._subscriptionId, diskName, comp, null, null));
		}

		public void DeleteHostedService(string serviceName)
		{
			this.SyncService.EndDeleteHostedService(this.SyncService.BeginDeleteHostedService(this._subscriptionId, serviceName, null, null));
		}

		public void DeleteHostedServiceExtension(string serviceName, string extensionId)
		{
			this.SyncService.EndDeleteHostedServiceExtension(this.SyncService.BeginDeleteHostedServiceExtension(this._subscriptionId, serviceName, extensionId, null, null));
		}

		public void DeleteOSImage(string imageName)
		{
			this.SyncService.EndDeleteOSImage(this.SyncService.BeginDeleteOSImage(this._subscriptionId, imageName, null, null));
		}

		public void DeleteOSImageEx(string imageName, string comp)
		{
			this.SyncService.EndDeleteOSImageEx(this.SyncService.BeginDeleteOSImageEx(this._subscriptionId, imageName, comp, null, null));
		}

		public void DeleteRole(string serviceName, string deploymentName, string roleName)
		{
			this.SyncService.EndDeleteRole(this.SyncService.BeginDeleteRole(this._subscriptionId, serviceName, deploymentName, roleName, null, null));
		}

		public void DeleteStorageService(string serviceName)
		{
			this.SyncService.EndDeleteStorageService(this.SyncService.BeginDeleteStorageService(this._subscriptionId, serviceName, null, null));
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			this._logger.LogDebugInformation("Cleaning up Service Management client.");
			if (!this._disposed && disposing)
			{
				if (this.Service != null)
				{
					IContextChannel contextChannel = this.Service.ToContextChannel();
					if (contextChannel != null && contextChannel.State != CommunicationState.Closed && contextChannel.State != CommunicationState.Closing)
					{
						if (contextChannel.State != CommunicationState.Faulted)
						{
							contextChannel.Close();
						}
						else
						{
							contextChannel.Abort();
						}
					}
					this._asyncService = null;
					IContextChannel contextChannel1 = this.SyncService.ToContextChannel();
					if (contextChannel1 != null && contextChannel1.State != CommunicationState.Closed && contextChannel1.State != CommunicationState.Closing)
					{
						if (contextChannel1.State != CommunicationState.Faulted)
						{
							contextChannel1.Close();
						}
						else
						{
							contextChannel1.Abort();
						}
					}
					this._syncService = null;
				}
				this._disposed = true;
				this._clientCert = null;
			}
		}

		public Stream DownloadRDPFile(string serviceName, string deploymentName, string roleInstanceName)
		{
			return this.SyncService.EndDownloadRDPFile(this.SyncService.BeginDownloadRDPFile(this._subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
		}

		public void ExecuteRoleSetOperation(string serviceName, string deploymentName, RoleSetOperation roleSetOperation)
		{
			this.SyncService.EndExecuteRoleSetOperation(this.SyncService.BeginExecuteRoleSetOperation(this._subscriptionId, serviceName, deploymentName, roleSetOperation, null, null));
		}

		public AffinityGroup GetAffinityGroup(string affinityGroupName)
		{
			return this.SyncService.EndGetAffinityGroup(this.SyncService.BeginGetAffinityGroup(this._subscriptionId, affinityGroupName, null, null));
		}

		public Certificate GetCertificate(string serviceName, string algorithm, string thumbprint)
		{
			return this.SyncService.EndGetCertificate(this.SyncService.BeginGetCertificate(this._subscriptionId, serviceName, algorithm, thumbprint, null, null));
		}

		public DataVirtualHardDisk GetDataDisk(string serviceName, string deploymentName, string roleName, int lun)
		{
			return this.SyncService.EndGetDataDisk(this.SyncService.BeginGetDataDisk(this._subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), null, null));
		}

		public Deployment GetDeployment(string serviceName, string deploymentName)
		{
			return this.SyncService.EndGetDeployment(this.SyncService.BeginGetDeployment(this._subscriptionId, serviceName, deploymentName, null, null));
		}

		public Deployment GetDeploymentBySlot(string serviceName, string deploymentSlot)
		{
			return this.SyncService.EndGetDeploymentBySlot(this.SyncService.BeginGetDeploymentBySlot(this._subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public Disk GetDisk(string diskName)
		{
			return this.SyncService.EndGetDisk(this.SyncService.BeginGetDisk(this._subscriptionId, diskName, null, null));
		}

		public HostedService GetHostedService(string serviceName)
		{
			return this.SyncService.EndGetHostedService(this.SyncService.BeginGetHostedService(this._subscriptionId, serviceName, null, null));
		}

		public HostedServiceExtension GetHostedServiceExtension(string serviceName, string extensionId)
		{
			return this.SyncService.EndGetHostedServiceExtension(this.SyncService.BeginGetHostedServiceExtension(this._subscriptionId, serviceName, extensionId, null, null));
		}

		public HostedService GetHostedServiceWithDetails(string serviceName, bool embedDetail)
		{
			return this.SyncService.EndGetHostedServiceWithDetails(this.SyncService.BeginGetHostedServiceWithDetails(this._subscriptionId, serviceName, embedDetail, null, null));
		}

		public Stream GetNetworkConfiguration()
		{
			return this.SyncService.EndGetNetworkConfiguration(this.SyncService.BeginGetNetworkConfiguration(this._subscriptionId, null, null));
		}

		public Operation GetOperationStatus(string operationId)
		{
			return this.SyncService.EndGetOperationStatus(this.SyncService.BeginGetOperationStatus(this._subscriptionId, operationId, null, null));
		}

		public OSImage GetOSImage(string imageName)
		{
			return this.SyncService.EndGetOSImage(this.SyncService.BeginGetOSImage(this._subscriptionId, imageName, null, null));
		}

		public OSImageDetails GetOSImagWithDetails(string imageName)
		{
			return this.SyncService.EndGetOSImageWithDetails(this.SyncService.BeginGetOSImageWithDetails(this._subscriptionId, imageName, null, null));
		}

		public void GetPackage(string serviceName, string deploymentName, string containerUri, bool overwriteExisting)
		{
			this.SyncService.EndGetPackage(this.SyncService.BeginGetPackage(this._subscriptionId, serviceName, deploymentName, containerUri, overwriteExisting, null, null));
		}

		public void GetPackageBySlot(string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting)
		{
			this.SyncService.EndGetPackageBySlot(this.SyncService.BeginGetPackageBySlot(this._subscriptionId, serviceName, deploymentSlot, containerUri, overwriteExisting, null, null));
		}

		public PersistentVMRole GetRole(string serviceName, string deploymentName, string roleName)
		{
			return (PersistentVMRole)this.SyncService.EndGetRole(this.SyncService.BeginGetRole(this._subscriptionId, serviceName, deploymentName, roleName, null, null));
		}

		public StorageService GetStorageKeys(string name)
		{
			return this.SyncService.EndGetStorageKeys(this.SyncService.BeginGetStorageKeys(this._subscriptionId, name, null, null));
		}

		public StorageService GetStorageService(string name)
		{
			return this.SyncService.EndGetStorageService(this.SyncService.BeginGetStorageService(this._subscriptionId, name, null, null));
		}

		public Subscription GetSubscription()
		{
			return this.SyncService.EndGetSubscription(this.SyncService.BeginGetSubscription(this._subscriptionId, null, null));
		}

		public SubscriptionCertificate GetSubscriptionCertificate(string thumbprint)
		{
			return this.SyncService.EndGetSubscriptionCertificate(this.SyncService.BeginGetSubscriptionCertificate(this._subscriptionId, thumbprint, null, null));
		}

		private void Initialize(Uri remoteUri, ServiceManagementClientOptions clientOptions)
		{
			if (this._disposed)
			{
				throw new InvalidOperationException(Resources.InitDisposedClientError);
			}
			this._logger = new TraceSourceHelper(clientOptions.Logger, clientOptions.ErrorEventId, "ServiceManagementClient");
			this._logger.LogDebugInformation("Logger has been successfully initialized for the ServiceManagementClient.");
			this._logger.LogDebugInformation("Constructing the binding for the client.");
			WebHttpBinding webHttpBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
			webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
			webHttpBinding.MaxReceivedMessageSize = (long)1048576;
			webHttpBinding.MaxBufferPoolSize = (long)1048576;
			XmlDictionaryReaderQuotas xmlDictionaryReaderQuota = new XmlDictionaryReaderQuotas()
			{
				MaxStringContentLength = 1048576,
				MaxBytesPerRead = 131072
			};
			webHttpBinding.ReaderQuotas = xmlDictionaryReaderQuota;
			TraceSourceHelper traceSourceHelper = this._logger;
			object[] maxReceivedMessageSize = new object[] { webHttpBinding.MaxReceivedMessageSize, webHttpBinding.MaxBufferPoolSize, webHttpBinding.ReaderQuotas.MaxStringContentLength, webHttpBinding.ReaderQuotas.MaxBytesPerRead };
			traceSourceHelper.LogDebugInformation("Binding - MaxReceivedMessageSize: {0}, MaxBufferPoolSize: {1}, ReaderQuotas.MaxStringContentLength: {2}, ReaderQuotas.MaxBytesPerRead: {3}.", maxReceivedMessageSize);
			webHttpBinding.ReceiveTimeout = TimeSpan.FromMinutes(5);
			webHttpBinding.SendTimeout = TimeSpan.FromMinutes(5);
			webHttpBinding.OpenTimeout = TimeSpan.FromMinutes(2);
			webHttpBinding.CloseTimeout = TimeSpan.FromMinutes(1);
			TraceSourceHelper traceSourceHelper1 = this._logger;
			object[] receiveTimeout = new object[] { webHttpBinding.ReceiveTimeout, webHttpBinding.SendTimeout, webHttpBinding.OpenTimeout, webHttpBinding.CloseTimeout };
			traceSourceHelper1.LogDebugInformation("Binding - ReceiveTimeout: {0}, SendTimeout: {1}, OpenTimeout: {2}, CloseTimeout: {3}.", receiveTimeout);
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(webHttpBinding, remoteUri);
			this._logger.LogDebugInformation("Adding custom ClientOutputMessageInspector.");
			webChannelFactory.Endpoint.Behaviors.Add(new ClientMessageInspector(clientOptions.UserAgentString, clientOptions.ClientRequestIdGenerator, clientOptions.Logger, clientOptions.ErrorEventId, clientOptions.MessageInspectors));
			TraceSourceHelper traceSourceHelper2 = this._logger;
			object[] thumbprint = new object[] { this._clientCert.Thumbprint };
			traceSourceHelper2.LogInformation("Using client certificate with thumbprint {0}.", thumbprint);
			webChannelFactory.Credentials.ClientCertificate.Certificate = this._clientCert;
			this._logger.LogDebugInformation("Creating custom ServiceManagemenChannelProxy for async.");
			ServiceManagementChannelProxy serviceManagementChannelProxy = new ServiceManagementChannelProxy(webChannelFactory.CreateChannel(), true, clientOptions);
			this._logger.LogDebugInformation("Returning custom async proxy channel to client.");
			this._asyncService = (IServiceManagement)serviceManagementChannelProxy.GetTransparentProxy();
			this._logger.LogDebugInformation("Creating custom ServiceManagemenChannelProxy for sync.");
			ServiceManagementChannelProxy serviceManagementChannelProxy1 = new ServiceManagementChannelProxy(webChannelFactory.CreateChannel(), false, clientOptions);
			this._logger.LogDebugInformation("Returning custom sync proxy channel to client.");
			this._syncService = (IServiceManagement)serviceManagementChannelProxy1.GetTransparentProxy();
			this._logger.LogInformation("Please use the following guidelines for understanding the log statements:");
			this._logger.LogInformation("\tREQUEST, HTTP verb, request URL, x-ms-client-id header, UserAgent header, WCF correlation state.");
			this._logger.LogInformation("\tRESPONSE, HTTP status code, Service Management error code, Service Management error message, x-ms-request-id header, WCF correlation state.");
			this._logger.LogInformation("\tPROXYERROR, HTTP status code, Service Management error code, Service Management error message, x-ms-request-id header");
		}

		public AvailabilityResponse IsDNSAvailable(string dnsname)
		{
			return this.SyncService.EndIsDNSAvailable(this.SyncService.BeginIsDNSAvailable(this._subscriptionId, dnsname, null, null));
		}

		public AvailabilityResponse IsStorageServiceAvailable(string serviceName)
		{
			return this.SyncService.EndIsStorageServiceAvailable(this.SyncService.BeginIsStorageServiceAvailable(this._subscriptionId, serviceName, null, null));
		}

		public AffinityGroupList ListAffinityGroups()
		{
			return this.SyncService.EndListAffinityGroups(this.SyncService.BeginListAffinityGroups(this._subscriptionId, null, null));
		}

		public CertificateList ListCertificates(string serviceName)
		{
			return this.SyncService.EndListCertificates(this.SyncService.BeginListCertificates(this._subscriptionId, serviceName, null, null));
		}

		public DiskList ListDisks()
		{
			return this.SyncService.EndListDisks(this.SyncService.BeginListDisks(this._subscriptionId, null, null));
		}

		public HostedServiceExtensionList ListHostedServiceExtensions(string serviceName)
		{
			return this.SyncService.EndListHostedServiceExtensions(this.SyncService.BeginListHostedServiceExtensions(this._subscriptionId, serviceName, null, null));
		}

		public HostedServiceList ListHostedServices()
		{
			return this.SyncService.EndListHostedServices(this.SyncService.BeginListHostedServices(this._subscriptionId, null, null));
		}

		public ExtensionImageList ListLatestExtensions()
		{
			return this.SyncService.EndListLatestExtensions(this.SyncService.BeginListLatestExtensions(this._subscriptionId, null, null));
		}

		public LocationList ListLocations()
		{
			return this.SyncService.EndListLocations(this.SyncService.BeginListLocations(this._subscriptionId, null, null));
		}

		public OperatingSystemFamilyList ListOperatingSystemFamilies()
		{
			return this.SyncService.EndListOperatingSystemFamilies(this.SyncService.BeginListOperatingSystemFamilies(this._subscriptionId, null, null));
		}

		public OperatingSystemList ListOperatingSystems()
		{
			return this.SyncService.EndListOperatingSystems(this.SyncService.BeginListOperatingSystems(this._subscriptionId, null, null));
		}

		public OSImageList ListOSImages()
		{
			return this.SyncService.EndListOSImages(this.SyncService.BeginListOSImages(this._subscriptionId, null, null));
		}

		public StorageServiceList ListStorageServices()
		{
			return this.SyncService.EndListStorageServices(this.SyncService.BeginListStorageServices(this._subscriptionId, null, null));
		}

		public SubscriptionCertificateList ListSubscriptionCertificates()
		{
			return this.SyncService.EndListSubscriptionCertificates(this.SyncService.BeginListSubscriptionCertificates(this._subscriptionId, null, null));
		}

		public SubscriptionOperationCollection ListSubscriptionOperations(string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken)
		{
			return this.SyncService.EndListSubscriptionOperations(this.SyncService.BeginListSubscriptionOperations(this._subscriptionId, startTime, endTime, objectIdFilter, operationResultFilter, continuationToken, null, null));
		}

		public VirtualNetworkSiteList ListVirtualNetworkSites()
		{
			return this.SyncService.EndListVirtualNetworkSites(this.SyncService.BeginListVirtualNetworkSites(this._subscriptionId, null, null));
		}

		public void RebootDeploymentRoleInstance(string serviceName, string deploymentName, string roleInstanceName)
		{
			this.SyncService.EndRebootDeploymentRoleInstance(this.SyncService.BeginRebootDeploymentRoleInstance(this._subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
		}

		public void RebootDeploymentRoleInstanceBySlot(string serviceName, string deploymentSlot, string roleInstanceName)
		{
			this.SyncService.EndRebootDeploymentRoleInstanceBySlot(this.SyncService.BeginRebootDeploymentRoleInstanceBySlot(this._subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
		}

		public void RebuildDeploymentRoleInstance(string serviceName, string deploymentName, string roleInstanceName, string resources)
		{
			this.SyncService.EndRebuildDeploymentRoleInstance(this.SyncService.BeginRebuildDeploymentRoleInstance(this._subscriptionId, serviceName, deploymentName, roleInstanceName, resources, null, null));
		}

		public void RebuildDeploymentRoleInstanceBySlot(string serviceName, string deploymentSlot, string roleInstanceName, string resources)
		{
			this.SyncService.EndRebuildDeploymentRoleInstanceBySlot(this.SyncService.BeginRebuildDeploymentRoleInstanceBySlot(this._subscriptionId, serviceName, deploymentSlot, roleInstanceName, resources, null, null));
		}

		public StorageService RegenerateStorageServiceKeys(string name, RegenerateKeys regenerateKeys)
		{
			return this.SyncService.EndRegenerateStorageServiceKeys(this.SyncService.BeginRegenerateStorageServiceKeys(this._subscriptionId, name, regenerateKeys, null, null));
		}

		public void ReimageDeploymentRoleInstance(string serviceName, string deploymentName, string roleInstanceName)
		{
			this.SyncService.EndReimageDeploymentRoleInstance(this.SyncService.BeginReimageDeploymentRoleInstance(this._subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
		}

		public void ReimageDeploymentRoleInstanceBySlot(string serviceName, string deploymentSlot, string roleInstanceName)
		{
			this.SyncService.EndReimageDeploymentRoleInstanceBySlot(this.SyncService.BeginReimageDeploymentRoleInstanceBySlot(this._subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
		}

		public void RemoveSubscriptionCertificate(string thumbprint)
		{
			this.SyncService.EndRemoveSubscriptionCertificate(this.SyncService.BeginRemoveSubscriptionCertificate(this._subscriptionId, thumbprint, null, null));
		}

		public string ReplicateOSImage(string imageName, ReplicationInput replicationInput)
		{
			return this.SyncService.EndReplicateOSImage(this.SyncService.BeginReplicateOSImage(this._subscriptionId, imageName, replicationInput, null, null));
		}

		public void RestartRole(string serviceName, string deploymentName, string roleInstanceName)
		{
			this.SyncService.EndExecuteRoleOperation(this.SyncService.BeginExecuteRoleOperation(this._subscriptionId, serviceName, deploymentName, roleInstanceName, new RestartRoleOperation(), null, null));
		}

		public void ResumeDeploymentUpdateOrUpgrade(string serviceName, string deploymentName)
		{
			this.SyncService.EndResumeDeploymentUpdateOrUpgrade(this.SyncService.BeginResumeDeploymentUpdateOrUpgrade(this._subscriptionId, serviceName, deploymentName, null, null));
		}

		public void ResumeDeploymentUpdateOrUpgradeBySlot(string serviceName, string deploymentSlot)
		{
			this.SyncService.EndResumeDeploymentUpdateOrUpgradeBySlot(this.SyncService.BeginResumeDeploymentUpdateOrUpgradeBySlot(this._subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public void RollbackDeploymentUpdateOrUpgrade(string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input)
		{
			this.SyncService.EndRollbackDeploymentUpdateOrUpgrade(this.SyncService.BeginRollbackDeploymentUpdateOrUpgrade(this._subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public void RollbackDeploymentUpdateOrUpgradeBySlot(string serviceName, string slotName, RollbackUpdateOrUpgradeInput input)
		{
			this.SyncService.EndRollbackDeploymentUpdateOrUpgradeBySlot(this.SyncService.BeginRollbackDeploymentUpdateOrUpgradeBySlot(this._subscriptionId, serviceName, slotName, input, null, null));
		}

		public void SetNetworkConfiguration(Stream networkConfiguration)
		{
			this.SyncService.EndSetNetworkConfiguration(this.SyncService.BeginSetNetworkConfiguration(this._subscriptionId, networkConfiguration, null, null));
		}

		public bool ShareOSImage(string imageName, string permission)
		{
			return this.SyncService.EndShareOSImage(this.SyncService.BeginShareOSImage(this._subscriptionId, imageName, permission, null, null));
		}

		public void ShutdownRole(string serviceName, string deploymentName, string roleInstanceName)
		{
			this.SyncService.EndExecuteRoleOperation(this.SyncService.BeginExecuteRoleOperation(this._subscriptionId, serviceName, deploymentName, roleInstanceName, new ShutdownRoleOperation(), null, null));
		}

		public void ShutdownRole(string serviceName, string deploymentName, string roleInstanceName, PostShutdownAction postShutdownAction)
		{
			IServiceManagement syncService = this.SyncService;
			IServiceManagement serviceManagement = this.SyncService;
			string str = this._subscriptionId;
			ShutdownRoleOperation shutdownRoleOperation = new ShutdownRoleOperation()
			{
				PostShutdownAction = new PostShutdownAction?(postShutdownAction)
			};
			syncService.EndExecuteRoleOperation(serviceManagement.BeginExecuteRoleOperation(str, serviceName, deploymentName, roleInstanceName, shutdownRoleOperation, null, null));
		}

		public void ShutdownRoles(string serviceName, string deploymentName, RoleNamesCollection roles, PostShutdownAction postShutdownAction)
		{
			IServiceManagement syncService = this.SyncService;
			IServiceManagement serviceManagement = this.SyncService;
			string str = this._subscriptionId;
			ShutdownRolesOperation shutdownRolesOperation = new ShutdownRolesOperation()
			{
				Roles = roles,
				PostShutdownAction = postShutdownAction
			};
			syncService.EndExecuteRoleSetOperation(serviceManagement.BeginExecuteRoleSetOperation(str, serviceName, deploymentName, shutdownRolesOperation, null, null));
		}

		public void StartRole(string serviceName, string deploymentName, string roleInstanceName)
		{
			this.SyncService.EndExecuteRoleOperation(this.SyncService.BeginExecuteRoleOperation(this._subscriptionId, serviceName, deploymentName, roleInstanceName, new StartRoleOperation(), null, null));
		}

		public void StartRoles(string serviceName, string deploymentName, RoleNamesCollection roles)
		{
			IServiceManagement syncService = this.SyncService;
			IServiceManagement serviceManagement = this.SyncService;
			string str = this._subscriptionId;
			StartRolesOperation startRolesOperation = new StartRolesOperation()
			{
				Roles = roles
			};
			syncService.EndExecuteRoleSetOperation(serviceManagement.BeginExecuteRoleSetOperation(str, serviceName, deploymentName, startRolesOperation, null, null));
		}

		public void SuspendDeploymentUpdateOrUpgrade(string serviceName, string deploymentName)
		{
			this.SyncService.EndSuspendDeploymentUpdateOrUpgrade(this.SyncService.BeginSuspendDeploymentUpdateOrUpgrade(this._subscriptionId, serviceName, deploymentName, null, null));
		}

		public void SuspendDeploymentUpdateOrUpgradeBySlot(string serviceName, string deploymentSlot)
		{
			this.SyncService.EndSuspendDeploymentUpdateOrUpgradeBySlot(this.SyncService.BeginSuspendDeploymentUpdateOrUpgradeBySlot(this._subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public void SwapDeployment(string serviceName, SwapDeploymentInput input)
		{
			this.SyncService.EndSwapDeployment(this.SyncService.BeginSwapDeployment(this._subscriptionId, serviceName, input, null, null));
		}

		public IContextChannel ToSynchronousContextChannel()
		{
			return this._syncService.ToContextChannel();
		}

		public void UnReplicateOSImage(string imageName)
		{
			this.SyncService.EndUnReplicateOSImage(this.SyncService.BeginUnReplicateOSImage(this._subscriptionId, imageName, null, null));
		}

		public void UpdateAffinityGroup(string affinityGroupName, UpdateAffinityGroupInput input)
		{
			this.SyncService.EndUpdateAffinityGroup(this.SyncService.BeginUpdateAffinityGroup(this._subscriptionId, affinityGroupName, input, null, null));
		}

		public void UpdateDataDisk(string serviceName, string deploymentName, string roleName, int lun, DataVirtualHardDisk dataDisk)
		{
			this.SyncService.EndUpdateDataDisk(this.SyncService.BeginUpdateDataDisk(this._subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), dataDisk, null, null));
		}

		public void UpdateDeploymentStatus(string serviceName, string deploymentName, UpdateDeploymentStatusInput input)
		{
			this.SyncService.EndUpdateDeploymentStatus(this.SyncService.BeginUpdateDeploymentStatus(this._subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public void UpdateDeploymentStatusBySlot(string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input)
		{
			this.SyncService.EndUpdateDeploymentStatusBySlot(this.SyncService.BeginUpdateDeploymentStatusBySlot(this._subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public Disk UpdateDisk(string diskName, Disk disk)
		{
			return this.SyncService.EndUpdateDisk(this.SyncService.BeginUpdateDisk(this._subscriptionId, diskName, disk, null, null));
		}

		public void UpdateHostedService(string serviceName, UpdateHostedServiceInput input)
		{
			this.SyncService.EndUpdateHostedService(this.SyncService.BeginUpdateHostedService(this._subscriptionId, serviceName, input, null, null));
		}

		public void UpdateLoadBalancedEndpointSet(string serviceName, string deploymentName, LoadBalancedEndpointList loadBalancedEndpointList)
		{
			this.SyncService.EndUpdateLoadBalancedEndpointSet(this.SyncService.BeginUpdateLoadBalancedEndpointSet(this._subscriptionId, serviceName, deploymentName, loadBalancedEndpointList, null, null));
		}

		public OSImage UpdateOSImage(string imageName, OSImage image)
		{
			return this.SyncService.EndUpdateOSImage(this.SyncService.BeginUpdateOSImage(this._subscriptionId, imageName, image, null, null));
		}

		public void UpdateRole(string serviceName, string deploymentName, string roleName, PersistentVMRole role)
		{
			this.SyncService.EndUpdateRole(this.SyncService.BeginUpdateRole(this._subscriptionId, serviceName, deploymentName, roleName, role, null, null));
		}

		public void UpdateStorageService(string serviceName, UpdateStorageServiceInput input)
		{
			this.SyncService.EndUpdateStorageService(this.SyncService.BeginUpdateStorageService(this._subscriptionId, serviceName, input, null, null));
		}

		public void UpgradeDeployment(string serviceName, string deploymentName, UpgradeDeploymentInput input)
		{
			this.SyncService.EndUpgradeDeployment(this.SyncService.BeginUpgradeDeployment(this._subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public void UpgradeDeploymentBySlot(string serviceName, string deploymentSlot, UpgradeDeploymentInput input)
		{
			this.SyncService.EndUpgradeDeploymentBySlot(this.SyncService.BeginUpgradeDeploymentBySlot(this._subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public void WalkUpgradeDomain(string serviceName, string deploymentName, WalkUpgradeDomainInput input)
		{
			this.SyncService.EndWalkUpgradeDomain(this.SyncService.BeginWalkUpgradeDomain(this._subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public void WalkUpgradeDomainBySlot(string serviceName, string deploymentSlot, WalkUpgradeDomainInput input)
		{
			this.SyncService.EndWalkUpgradeDomainBySlot(this.SyncService.BeginWalkUpgradeDomainBySlot(this._subscriptionId, serviceName, deploymentSlot, input, null, null));
		}
	}
}