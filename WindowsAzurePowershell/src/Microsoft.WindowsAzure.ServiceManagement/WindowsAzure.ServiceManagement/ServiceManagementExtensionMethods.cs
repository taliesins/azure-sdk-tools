using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	public static class ServiceManagementExtensionMethods
	{
		public static Task AddCertificatesTask(this IServiceManagement proxy, string subscriptionId, string serviceName, CertificateFile input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginAddCertificates(subscriptionId, serviceName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndAddCertificates));
		}

		public static Task AddDataDiskTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, DataVirtualHardDisk dataDisk)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginAddDataDisk(subscriptionId, serviceName, deploymentName, roleName, dataDisk, null, null), new Action<IAsyncResult>(serviceManagement.EndAddDataDisk));
		}

		public static Task AddHostedServiceExtensionTask(this IServiceManagement proxy, string subscriptionId, string serviceName, HostedServiceExtensionInput extension)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginAddHostedServiceExtension(subscriptionId, serviceName, extension, null, null), new Action<IAsyncResult>(serviceManagement.EndAddHostedServiceExtension));
		}

		public static Task AddRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, PersistentVMRole role)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginAddRole(subscriptionId, serviceName, deploymentName, role, null, null), new Action<IAsyncResult>(serviceManagement.EndAddRole));
		}

		public static Task AddSubscriptionCertificateTask(this IServiceManagement proxy, string subscriptionId, SubscriptionCertificate certificate)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginAddSubscriptionCertificate(subscriptionId, certificate, null, null), new Action<IAsyncResult>(serviceManagement.EndAddSubscriptionCertificate));
		}

		public static Task CaptureRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, string targetImageName, string targetImageLabel, PostCaptureAction postCaptureAction, ProvisioningConfigurationSet provisioningConfiguration)
		{
			TaskFactory factory = Task.Factory;
			CaptureRoleOperation captureRoleOperation = new CaptureRoleOperation()
			{
				PostCaptureAction = postCaptureAction.ToString(),
				ProvisioningConfiguration = provisioningConfiguration,
				TargetImageName = targetImageName,
				TargetImageLabel = targetImageLabel
			};
			IServiceManagement serviceManagement = proxy;
			return factory.FromAsync(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, captureRoleOperation, null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleOperation));
		}

		public static Task ChangeConfigurationBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, ChangeConfigurationInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginChangeConfigurationBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null), new Action<IAsyncResult>(serviceManagement.EndChangeConfigurationBySlot));
		}

		public static Task ChangeConfigurationTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, ChangeConfigurationInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginChangeConfiguration(subscriptionId, serviceName, deploymentName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndChangeConfiguration));
		}

		public static Task CreateAffinityGroupTask(this IServiceManagement proxy, string subscriptionId, CreateAffinityGroupInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginCreateAffinityGroup(subscriptionId, input, null, null), new Action<IAsyncResult>(serviceManagement.EndCreateAffinityGroup));
		}

		public static Task CreateDeploymentTask(this IServiceManagement proxy, string subscriptionId, string serviceName, Deployment deployment)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginCreateDeployment(subscriptionId, serviceName, deployment, null, null), new Action<IAsyncResult>(serviceManagement.EndCreateDeployment));
		}

		public static Task<Disk> CreateDiskTask(this IServiceManagement proxy, string subscriptionID, Disk disk)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Disk>(proxy.BeginCreateDisk(subscriptionID, disk, null, null), new Func<IAsyncResult, Disk>(serviceManagement.EndCreateDisk));
		}

		public static Task CreateHostedServiceTask(this IServiceManagement proxy, string subscriptionId, CreateHostedServiceInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginCreateHostedService(subscriptionId, input, null, null), new Action<IAsyncResult>(serviceManagement.EndCreateHostedService));
		}

		public static Task CreateOrUpdateDeploymentTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, CreateDeploymentInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginCreateOrUpdateDeployment(subscriptionId, serviceName, deploymentSlot, input, null, null), new Action<IAsyncResult>(serviceManagement.EndCreateOrUpdateDeployment));
		}

		public static Task<OSImage> CreateOSImageTask(this IServiceManagement proxy, string subscriptionID, OSImage image)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OSImage>(proxy.BeginCreateOSImage(subscriptionID, image, null, null), new Func<IAsyncResult, OSImage>(serviceManagement.EndCreateOSImage));
		}

		public static Task CreateStorageServiceTask(this IServiceManagement proxy, string subscriptionId, CreateStorageServiceInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginCreateStorageService(subscriptionId, input, null, null), new Action<IAsyncResult>(serviceManagement.EndCreateStorageService));
		}

		public static Task DeleteAffinityGroupTask(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteAffinityGroup(subscriptionId, affinityGroupName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteAffinityGroup));
		}

		public static Task DeleteCertificateTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteCertificate));
		}

		public static Task DeleteDataDiskTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, int lun)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteDataDisk(subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteDataDisk));
		}

		public static Task DeleteDeploymentBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteDeploymentBySlot));
		}

		public static Task DeleteDeploymentTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteDeployment(subscriptionId, serviceName, deploymentName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteDeployment));
		}

		public static void DeleteDeploymentVirtualIPs(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string vipGroupName, VirtualIPList vips)
		{
			proxy.EndDeleteDeploymentVirtualIPs(proxy.BeginDeleteDeploymentVirtualIPs(subscriptionId, serviceName, deploymentName, vipGroupName, vips, null, null));
		}

		public static void DeleteDeploymentVirtualIPsBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string vipGroupName, VirtualIPList vips)
		{
			proxy.EndDeleteDeploymentVirtualIPsBySlot(proxy.BeginDeleteDeploymentVirtualIPsBySlot(subscriptionId, serviceName, deploymentSlot, vipGroupName, vips, null, null));
		}

		public static Task DeleteDiskExTask(this IServiceManagement proxy, string subscriptionID, string diskName, string comp)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteDiskEx(subscriptionID, diskName, comp, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteDiskEx));
		}

		public static Task DeleteDiskTask(this IServiceManagement proxy, string subscriptionID, string diskName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteDisk(subscriptionID, diskName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteDisk));
		}

		public static Task DeleteHostedServiceExtensionTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string extensionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteHostedServiceExtension(subscriptionId, serviceName, extensionId, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteHostedServiceExtension));
		}

		public static Task DeleteHostedServiceTask(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteHostedService(subscriptionId, serviceName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteHostedService));
		}

		public static Task DeleteOSImageExTask(this IServiceManagement proxy, string subscriptionID, string imageName, string comp)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteOSImageEx(subscriptionID, imageName, comp, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteOSImageEx));
		}

		public static Task DeleteOSImageTask(this IServiceManagement proxy, string subscriptionID, string imageName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteOSImage(subscriptionID, imageName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteOSImage));
		}

		public static Task DeleteRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteRole(subscriptionId, serviceName, deploymentName, roleName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteRole));
		}

		public static Task DeleteStorageServiceTask(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginDeleteStorageService(subscriptionId, serviceName, null, null), new Action<IAsyncResult>(serviceManagement.EndDeleteStorageService));
		}

		public static Task<Stream> DownloadRDPFileTask(this IServiceManagement proxy, string subscriptionID, string serviceName, string deploymentName, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Stream>(proxy.BeginDownloadRDPFile(subscriptionID, serviceName, deploymentName, roleInstanceName, null, null), new Func<IAsyncResult, Stream>(serviceManagement.EndDownloadRDPFile));
		}

		public static Task<AffinityGroup> GetAffinityGroupTask(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<AffinityGroup>(proxy.BeginGetAffinityGroup(subscriptionId, affinityGroupName, null, null), new Func<IAsyncResult, AffinityGroup>(serviceManagement.EndGetAffinityGroup));
		}

		public static Task<Certificate> GetCertificateTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Certificate>(proxy.BeginGetCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null), new Func<IAsyncResult, Certificate>(serviceManagement.EndGetCertificate));
		}

		public static Task<DataVirtualHardDisk> GetDataDiskTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, int lun)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<DataVirtualHardDisk>(proxy.BeginGetDataDisk(subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), null, null), new Func<IAsyncResult, DataVirtualHardDisk>(serviceManagement.EndGetDataDisk));
		}

		public static Task<Deployment> GetDeploymentBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Deployment>(proxy.BeginGetDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null), new Func<IAsyncResult, Deployment>(serviceManagement.EndGetDeploymentBySlot));
		}

		public static Task<Deployment> GetDeploymentTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Deployment>(proxy.BeginGetDeployment(subscriptionId, serviceName, deploymentName, null, null), new Func<IAsyncResult, Deployment>(serviceManagement.EndGetDeployment));
		}

		public static Task<Disk> GetDiskTask(this IServiceManagement proxy, string subscriptionID, string diskName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Disk>(proxy.BeginGetDisk(subscriptionID, diskName, null, null), new Func<IAsyncResult, Disk>(serviceManagement.EndGetDisk));
		}

		public static Task<HostedServiceExtension> GetHostedServiceExtensionTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string extensionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<HostedServiceExtension>(proxy.BeginGetHostedServiceExtension(subscriptionId, serviceName, extensionId, null, null), new Func<IAsyncResult, HostedServiceExtension>(serviceManagement.EndGetHostedServiceExtension));
		}

		public static Task<HostedService> GetHostedServiceTask(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<HostedService>(proxy.BeginGetHostedService(subscriptionId, serviceName, null, null), new Func<IAsyncResult, HostedService>(serviceManagement.EndGetHostedService));
		}

		public static Task<HostedService> GetHostedServiceWithDetailsTask(this IServiceManagement proxy, string subscriptionId, string serviceName, bool embedDetail)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<HostedService>(proxy.BeginGetHostedServiceWithDetails(subscriptionId, serviceName, embedDetail, null, null), new Func<IAsyncResult, HostedService>(serviceManagement.EndGetHostedServiceWithDetails));
		}

		public static Task<Stream> GetNetworkConfigurationTask(this IServiceManagement proxy, string subscriptionID)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Stream>(proxy.BeginGetNetworkConfiguration(subscriptionID, null, null), new Func<IAsyncResult, Stream>(serviceManagement.EndGetNetworkConfiguration));
		}

		public static Task<Operation> GetOperationStatusTask(this IServiceManagement proxy, string subscriptionId, string operationId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Operation>(proxy.BeginGetOperationStatus(subscriptionId, operationId, null, null), new Func<IAsyncResult, Operation>(serviceManagement.EndGetOperationStatus));
		}

		public static Task<OSImage> GetOSImageTask(this IServiceManagement proxy, string subscriptionID, string imageName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OSImage>(proxy.BeginGetOSImage(subscriptionID, imageName, null, null), new Func<IAsyncResult, OSImage>(serviceManagement.EndGetOSImage));
		}

		public static Task<OSImageDetails> GetOSImagWithDetailsTask(this IServiceManagement proxy, string subscriptionId, string imageName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OSImageDetails>(proxy.BeginGetOSImageWithDetails(subscriptionId, imageName, null, null), new Func<IAsyncResult, OSImageDetails>(serviceManagement.EndGetOSImageWithDetails));
		}

		public static Task GetPackageBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginGetPackageBySlot(subscriptionId, serviceName, deploymentSlot, containerUri, overwriteExisting, null, null), new Action<IAsyncResult>(serviceManagement.EndGetPackageBySlot));
		}

		public static Task GetPackageTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string containerUri, bool overwriteExisting)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginGetPackage(subscriptionId, serviceName, deploymentName, containerUri, overwriteExisting, null, null), new Action<IAsyncResult>(serviceManagement.EndGetPackage));
		}

		public static Task<PersistentVMRole> GetRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName)
		{
			return Task.Factory.FromAsync<PersistentVMRole>(proxy.BeginGetRole(subscriptionId, serviceName, deploymentName, roleName, null, null), (IAsyncResult asyncResult) => (PersistentVMRole)proxy.EndGetRole(asyncResult));
		}

		public static Task<StorageService> GetStorageKeysTask(this IServiceManagement proxy, string subscriptionId, string name)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<StorageService>(proxy.BeginGetStorageKeys(subscriptionId, name, null, null), new Func<IAsyncResult, StorageService>(serviceManagement.EndGetStorageKeys));
		}

		public static Task<StorageService> GetStorageServiceTask(this IServiceManagement proxy, string subscriptionId, string name)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<StorageService>(proxy.BeginGetStorageService(subscriptionId, name, null, null), new Func<IAsyncResult, StorageService>(serviceManagement.EndGetStorageService));
		}

		public static Task<SubscriptionCertificate> GetSubscriptionCertificateTask(this IServiceManagement proxy, string subscriptionID, string thumbprint)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<SubscriptionCertificate>(proxy.BeginGetSubscriptionCertificate(subscriptionID, thumbprint, null, null), new Func<IAsyncResult, SubscriptionCertificate>(serviceManagement.EndGetSubscriptionCertificate));
		}

		public static Task<Subscription> GetSubscriptionTask(this IServiceManagement proxy, string subscriptionID)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Subscription>(proxy.BeginGetSubscription(subscriptionID, null, null), new Func<IAsyncResult, Subscription>(serviceManagement.EndGetSubscription));
		}

		public static Task<AvailabilityResponse> IsDNSAvailableTask(this IServiceManagement proxy, string subscriptionID, string dnsname)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<AvailabilityResponse>(proxy.BeginIsDNSAvailable(subscriptionID, dnsname, null, null), new Func<IAsyncResult, AvailabilityResponse>(serviceManagement.EndIsDNSAvailable));
		}

		public static Task<AvailabilityResponse> IsStorageServiceAvailableTask(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<AvailabilityResponse>(proxy.BeginIsStorageServiceAvailable(subscriptionId, serviceName, null, null), new Func<IAsyncResult, AvailabilityResponse>(serviceManagement.EndIsStorageServiceAvailable));
		}

		public static Task<AffinityGroupList> ListAffinityGroupsTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<AffinityGroupList>(proxy.BeginListAffinityGroups(subscriptionId, null, null), new Func<IAsyncResult, AffinityGroupList>(serviceManagement.EndListAffinityGroups));
		}

		public static Task<CertificateList> ListCertificatesTask(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<CertificateList>(proxy.BeginListCertificates(subscriptionId, serviceName, null, null), new Func<IAsyncResult, CertificateList>(serviceManagement.EndListCertificates));
		}

		public static Task<DiskList> ListDisksTask(this IServiceManagement proxy, string subscriptionID)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<DiskList>(proxy.BeginListDisks(subscriptionID, null, null), new Func<IAsyncResult, DiskList>(serviceManagement.EndListDisks));
		}

		public static Task<HostedServiceExtensionList> ListHostedServiceExtensionsTask(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<HostedServiceExtensionList>(proxy.BeginListHostedServiceExtensions(subscriptionId, serviceName, null, null), new Func<IAsyncResult, HostedServiceExtensionList>(serviceManagement.EndListHostedServiceExtensions));
		}

		public static Task<HostedServiceList> ListHostedServicesTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<HostedServiceList>(proxy.BeginListHostedServices(subscriptionId, null, null), new Func<IAsyncResult, HostedServiceList>(serviceManagement.EndListHostedServices));
		}

		public static HostedServiceList ListHostedServicesWithDetails(this IServiceManagement proxy, string subscriptionId, ref string continuationToken)
		{
			HostedServiceList hostedServiceList;
			WebOperationContext current = WebOperationContext.Current;
			OperationContextScope operationContextScope = null;
			if (current == null)
			{
				operationContextScope = new OperationContextScope(proxy.ToContextChannel());
			}
			using (operationContextScope)
			{
				if (continuationToken == null)
				{
					WebOperationContext.Current.OutgoingRequest.Headers["x-ms-continuation-token"] = "All";
				}
				else
				{
					WebOperationContext.Current.OutgoingRequest.Headers["x-ms-continuation-token"] = continuationToken;
				}
				HostedServiceList hostedServiceList1 = proxy.EndListHostedServices(proxy.BeginListHostedServices(subscriptionId, null, null));
				continuationToken = WebOperationContext.Current.IncomingResponse.Headers["x-ms-continuation-token"];
				hostedServiceList = hostedServiceList1;
			}
			return hostedServiceList;
		}

		public static Task<ExtensionImageList> ListLatestExtensionsTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<ExtensionImageList>(proxy.BeginListLatestExtensions(subscriptionId, null, null), new Func<IAsyncResult, ExtensionImageList>(serviceManagement.EndListLatestExtensions));
		}

		public static Task<LocationList> ListLocationsTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<LocationList>(proxy.BeginListLocations(subscriptionId, null, null), new Func<IAsyncResult, LocationList>(serviceManagement.EndListLocations));
		}

		public static Task<OperatingSystemFamilyList> ListOperatingSystemFamiliesTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OperatingSystemFamilyList>(proxy.BeginListOperatingSystemFamilies(subscriptionId, null, null), new Func<IAsyncResult, OperatingSystemFamilyList>(serviceManagement.EndListOperatingSystemFamilies));
		}

		public static Task<OperatingSystemList> ListOperatingSystemsTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OperatingSystemList>(proxy.BeginListOperatingSystems(subscriptionId, null, null), new Func<IAsyncResult, OperatingSystemList>(serviceManagement.EndListOperatingSystems));
		}

		public static Task<OSImageList> ListOSImagesTask(this IServiceManagement proxy, string subscriptionID)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OSImageList>(proxy.BeginListOSImages(subscriptionID, null, null), new Func<IAsyncResult, OSImageList>(serviceManagement.EndListOSImages));
		}

		public static Task<StorageServiceList> ListStorageServicesTask(this IServiceManagement proxy, string subscriptionId)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<StorageServiceList>(proxy.BeginListStorageServices(subscriptionId, null, null), new Func<IAsyncResult, StorageServiceList>(serviceManagement.EndListStorageServices));
		}

		public static Task<SubscriptionCertificateList> ListSubscriptionCertificatesTask(this IServiceManagement proxy, string subscriptionID)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<SubscriptionCertificateList>(proxy.BeginListSubscriptionCertificates(subscriptionID, null, null), new Func<IAsyncResult, SubscriptionCertificateList>(serviceManagement.EndListSubscriptionCertificates));
		}

		public static Task<SubscriptionOperationCollection> ListSubscriptionOperationsTask(this IServiceManagement proxy, string subscriptionID, string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<SubscriptionOperationCollection>(proxy.BeginListSubscriptionOperations(subscriptionID, startTime, endTime, objectIdFilter, operationResultFilter, continuationToken, null, null), new Func<IAsyncResult, SubscriptionOperationCollection>(serviceManagement.EndListSubscriptionOperations));
		}

		public static Task<VirtualNetworkSiteList> ListVirtualNetworkSitesTask(this IServiceManagement proxy, string subscriptionID)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<VirtualNetworkSiteList>(proxy.BeginListVirtualNetworkSites(subscriptionID, null, null), new Func<IAsyncResult, VirtualNetworkSiteList>(serviceManagement.EndListVirtualNetworkSites));
		}

		public static Task RebootDeploymentRoleInstanceBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRebootDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null), new Action<IAsyncResult>(serviceManagement.EndRebootDeploymentRoleInstanceBySlot));
		}

		public static Task RebootDeploymentRoleInstanceTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRebootDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null), new Action<IAsyncResult>(serviceManagement.EndRebootDeploymentRoleInstance));
		}

		public static Task RebuildDeploymentRoleInstanceBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName, string resources)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRebuildDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, resources, null, null), new Action<IAsyncResult>(serviceManagement.EndRebuildDeploymentRoleInstanceBySlot));
		}

		public static Task RebuildDeploymentRoleInstanceTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, string resources)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRebuildDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, resources, null, null), new Action<IAsyncResult>(serviceManagement.EndRebuildDeploymentRoleInstance));
		}

		public static Task<StorageService> RegenerateStorageServiceKeysTask(this IServiceManagement proxy, string subscriptionId, string name, RegenerateKeys regenerateKeys)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<StorageService>(proxy.BeginRegenerateStorageServiceKeys(subscriptionId, name, regenerateKeys, null, null), new Func<IAsyncResult, StorageService>(serviceManagement.EndRegenerateStorageServiceKeys));
		}

		public static Task ReimageDeploymentRoleInstanceBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginReimageDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null), new Action<IAsyncResult>(serviceManagement.EndReimageDeploymentRoleInstanceBySlot));
		}

		public static Task ReimageDeploymentRoleInstanceTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginReimageDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null), new Action<IAsyncResult>(serviceManagement.EndReimageDeploymentRoleInstance));
		}

		public static Task RemoveSubscriptionCertificateTask(this IServiceManagement proxy, string subscriptionID, string thumbprint)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRemoveSubscriptionCertificate(subscriptionID, thumbprint, null, null), new Action<IAsyncResult>(serviceManagement.EndRemoveSubscriptionCertificate));
		}

		public static Task<string> ReplicateOSImageTask(this IServiceManagement proxy, string subscriptionId, string imageName, ReplicationInput replicationInput)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<string>(proxy.BeginReplicateOSImage(subscriptionId, imageName, replicationInput, null, null), new Func<IAsyncResult, string>(serviceManagement.EndReplicateOSImage));
		}

		public static Task RestartRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, new RestartRoleOperation(), null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleOperation));
		}

		public static Task ResumeDeploymentUpdateOrUpgradeBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginResumeDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null), new Action<IAsyncResult>(serviceManagement.EndResumeDeploymentUpdateOrUpgradeBySlot));
		}

		public static Task ResumeDeploymentUpdateOrUpgradeTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginResumeDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null), new Action<IAsyncResult>(serviceManagement.EndResumeDeploymentUpdateOrUpgrade));
		}

		public static Task RollbackDeploymentUpdateOrUpgradeBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string slotName, RollbackUpdateOrUpgradeInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRollbackDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, slotName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndRollbackDeploymentUpdateOrUpgradeBySlot));
		}

		public static Task RollbackDeploymentUpdateOrUpgradeTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginRollbackDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndRollbackDeploymentUpdateOrUpgrade));
		}

		public static Task SetNetworkConfigurationTask(this IServiceManagement proxy, string subscriptionID, Stream networkConfiguration)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginSetNetworkConfiguration(subscriptionID, networkConfiguration, null, null), new Action<IAsyncResult>(serviceManagement.EndSetNetworkConfiguration));
		}

		public static Task<bool> ShareOSImageTask(this IServiceManagement proxy, string subscriptionId, string imageName, string permission)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<bool>(proxy.BeginShareOSImage(subscriptionId, imageName, permission, null, null), new Func<IAsyncResult, bool>(serviceManagement.EndShareOSImage));
		}

		public static Task ShutdownRolesTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, RoleNamesCollection roles, PostShutdownAction postShutdownAction)
		{
			TaskFactory factory = Task.Factory;
			ShutdownRolesOperation shutdownRolesOperation = new ShutdownRolesOperation()
			{
				Roles = roles,
				PostShutdownAction = postShutdownAction
			};
			IServiceManagement serviceManagement = proxy;
			return factory.FromAsync(proxy.BeginExecuteRoleSetOperation(subscriptionId, serviceName, deploymentName, shutdownRolesOperation, null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleSetOperation));
		}

		public static Task ShutdownRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, new ShutdownRoleOperation(), null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleOperation));
		}

		public static Task ShutdownRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, PostShutdownAction postShutdownAction)
		{
			TaskFactory factory = Task.Factory;
			ShutdownRoleOperation shutdownRoleOperation = new ShutdownRoleOperation()
			{
				PostShutdownAction = new PostShutdownAction?(postShutdownAction)
			};
			IServiceManagement serviceManagement = proxy;
			return factory.FromAsync(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, shutdownRoleOperation, null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleOperation));
		}

		public static Task StartRolesTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, RoleNamesCollection roles)
		{
			TaskFactory factory = Task.Factory;
			StartRolesOperation startRolesOperation = new StartRolesOperation()
			{
				Roles = roles
			};
			IServiceManagement serviceManagement = proxy;
			return factory.FromAsync(proxy.BeginExecuteRoleSetOperation(subscriptionId, serviceName, deploymentName, startRolesOperation, null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleSetOperation));
		}

		public static Task StartRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, new StartRoleOperation(), null, null), new Action<IAsyncResult>(serviceManagement.EndExecuteRoleOperation));
		}

		public static Task SuspendDeploymentUpdateOrUpgradeBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginSuspendDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null), new Action<IAsyncResult>(serviceManagement.EndSuspendDeploymentUpdateOrUpgradeBySlot));
		}

		public static Task SuspendDeploymentUpdateOrUpgradeTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginSuspendDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null), new Action<IAsyncResult>(serviceManagement.EndSuspendDeploymentUpdateOrUpgrade));
		}

		public static Task SwapDeploymentTask(this IServiceManagement proxy, string subscriptionId, string serviceName, SwapDeploymentInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginSwapDeployment(subscriptionId, serviceName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndSwapDeployment));
		}

		public static Task UnReplicateOSImageTask(this IServiceManagement proxy, string subscriptionId, string imageName)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUnReplicateOSImage(subscriptionId, imageName, null, null), new Action<IAsyncResult>(serviceManagement.EndUnReplicateOSImage));
		}

		public static Task UpdateAffinityGroupTask(this IServiceManagement proxy, string subscriptionId, string affinityGroupName, UpdateAffinityGroupInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateAffinityGroup(subscriptionId, affinityGroupName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateAffinityGroup));
		}

		public static Task UpdateDataDiskTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, int lun, DataVirtualHardDisk dataDisk)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateDataDisk(subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), dataDisk, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateDataDisk));
		}

		public static Task UpdateDeploymentStatusBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateDeploymentStatusBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateDeploymentStatusBySlot));
		}

		public static Task UpdateDeploymentStatusTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpdateDeploymentStatusInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateDeploymentStatus(subscriptionId, serviceName, deploymentName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateDeploymentStatus));
		}

		public static Task<Disk> UpdateDiskTask(this IServiceManagement proxy, string subscriptionID, string diskName, Disk disk)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<Disk>(proxy.BeginUpdateDisk(subscriptionID, diskName, disk, null, null), new Func<IAsyncResult, Disk>(serviceManagement.EndUpdateDisk));
		}

		public static Task UpdateHostedServiceTask(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateHostedServiceInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateHostedService(subscriptionId, serviceName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateHostedService));
		}

		public static Task UpdateLoadBalancedEndPointSetTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, LoadBalancedEndpointList loadBalancedEndpointList)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateLoadBalancedEndpointSet(subscriptionId, serviceName, deploymentName, loadBalancedEndpointList, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateLoadBalancedEndpointSet));
		}

		public static Task<OSImage> UpdateOSImageTask(this IServiceManagement proxy, string subscriptionID, string imageName, OSImage image)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync<OSImage>(proxy.BeginUpdateOSImage(subscriptionID, imageName, image, null, null), new Func<IAsyncResult, OSImage>(serviceManagement.EndUpdateOSImage));
		}

		public static Task UpdateRoleTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, PersistentVMRole role)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateRole(subscriptionId, serviceName, deploymentName, roleName, role, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateRole));
		}

		public static Task UpdateStorageServiceTask(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateStorageServiceInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpdateStorageService(subscriptionId, serviceName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpdateStorageService));
		}

		public static Task UpgradeDeploymentBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpgradeDeploymentInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpgradeDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpgradeDeploymentBySlot));
		}

		public static Task UpgradeDeploymentTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpgradeDeploymentInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginUpgradeDeployment(subscriptionId, serviceName, deploymentName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndUpgradeDeployment));
		}

		public static Task WalkUpgradeDomainBySlotTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, WalkUpgradeDomainInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginWalkUpgradeDomainBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null), new Action<IAsyncResult>(serviceManagement.EndWalkUpgradeDomainBySlot));
		}

		public static Task WalkUpgradeDomainTask(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, WalkUpgradeDomainInput input)
		{
			IServiceManagement serviceManagement = proxy;
			return Task.Factory.FromAsync(proxy.BeginWalkUpgradeDomain(subscriptionId, serviceName, deploymentName, input, null, null), new Action<IAsyncResult>(serviceManagement.EndWalkUpgradeDomain));
		}
	}
}