﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Utilities
{
    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;
    using Commands.Utilities.CloudService;
    using Commands.Utilities.Common;
    using Management.Compute.Models;
    using Management.Models;
    using Management.Storage;
    using Management.Storage.Models;
    using Moq;
    using Storage.Blob;
    using OperationStatus = Management.Compute.Models.OperationStatus;

    [TestClass]
    public class CloudServiceClientTests : TestBase
    {
        private WindowsAzureSubscription subscription;

        private ClientMocks clientMocks;

        private Mock<CloudBlobUtility> cloudBlobUtilityMock;

        private ICloudServiceClient client;

        private const string serviceName = "cloudService";

        private const string storageName = "storagename";

        private MockServicesHost services;
        private MockStorageService storageService;

        private void ExecuteInTempCurrentDirectory(string path, Action action)
        {
            string currentDirectory = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = path;
                action();
            }
            catch
            {
                Environment.CurrentDirectory = currentDirectory;
                throw;
            }
        }

        private void SetupStorage(string name, MockStorageService.StorageAccountData a)
        {
            a.Name = name;
            a.BlobEndpoint = "http://awesome.blob.core.windows.net/";
            a.QueueEndpoint = "http://awesome.queue.core.windows.net/";
            a.TableEndpoint = "http://awesome.table.core.windows.net/";
            a.PrimaryKey =
                "MNao3bm7t7B/x+g2/ssh9HnG0mEh1QV5EHpcna8CetYn+TSRoA8/SBoH6B3Ufwtnz3jZLSw9GEUuCTr3VooBWq==";
            a.SecondaryKey = "secondaryKey";
        }

        private void RemoveDeployments()
        {
            services.Clear()
                .Add(s => { s.Name = serviceName; });
        }

        [TestInitialize]
        public void TestSetup()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;

            storageService = new MockStorageService()
                .Add(a => SetupStorage(serviceName.ToLowerInvariant(), a))
                .Add(a => SetupStorage(storageName.ToLowerInvariant(), a));

            services = new MockServicesHost()
                .Add(s =>
                {
                    s.Name = serviceName;
                    s.AddDeployment(d =>
                    {
                        d.Slot = DeploymentSlot.Production;
                        d.Name = "mydeployment";
                    });
                });

            subscription = new WindowsAzureSubscription
            {
                Certificate = It.IsAny<X509Certificate2>(),
                IsDefault = true,
                ServiceEndpoint = new Uri("https://www.azure.com"),
                SubscriptionId = Guid.NewGuid().ToString(),
                SubscriptionName = Data.Subscription1,
            };

            cloudBlobUtilityMock = new Mock<CloudBlobUtility>();
            cloudBlobUtilityMock.Setup(f => f.UploadPackageToBlob(
                It.IsAny<StorageManagementClient>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BlobRequestOptions>())).Returns(new Uri("http://www.packageurl.azure.com"));

            clientMocks = new ClientMocks(subscription.SubscriptionId);

            services.InitializeMocks(clientMocks.ComputeManagementClientMock);
            storageService.InitializeMocks(clientMocks.StorageManagementClientMock);

            client = new CloudServiceClient(subscription,
                clientMocks.ManagementClientMock.Object,
                clientMocks.StorageManagementClientMock.Object,
                clientMocks.ComputeManagementClientMock.Object
                )
            {
                CloudBlobUtility = cloudBlobUtilityMock.Object
            };
        }

        [TestMethod]
        public void TestStartCloudService()
        {
            client.StartCloudService(serviceName);

            Assert.IsTrue(services.LastDeploymentStatusUpdate.HasValue);
            Assert.AreEqual(UpdatedDeploymentStatus.Running, services.LastDeploymentStatusUpdate.Value);
        }

        [TestMethod]
        public void TestStopCloudService()
        {
            client.StopCloudService(serviceName);

            Assert.IsTrue(services.LastDeploymentStatusUpdate.HasValue);
            Assert.AreEqual(UpdatedDeploymentStatus.Suspended, services.LastDeploymentStatusUpdate.Value);
        }

        [TestMethod]
        public void TestRemoveCloudService()
        {
            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.DeleteByNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns((string s, string deploymentName, bool deleteAll, CancellationToken cancellationToken) => Tasks.FromResult(
                    CreateComputeOperationResponse("req0")));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.HostedServices.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request000",
                    StatusCode = HttpStatusCode.OK
                }));

            // Test
            client.RemoveCloudService(serviceName);

            // Assert
            clientMocks.ComputeManagementClientMock.Verify(
                c => c.Deployments.DeleteByNameAsync(serviceName, "mydeployment", false, It.IsAny<CancellationToken>()), Times.Once);

            clientMocks.ComputeManagementClientMock.Verify(
                c => c.HostedServices.DeleteAsync(serviceName, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public void TestRemoveCloudServiceWithStaging()
        {
            services.Clear()
                .Add(s =>
                {
                    s.Name = serviceName;
                    s.AddDeployment(d =>
                    {
                        d.Name = "myStagingdeployment";
                        d.Slot = DeploymentSlot.Staging;
                    });
                });

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.DeleteByNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns((string s, string deploymentName, bool deleteAll, CancellationToken cancellationToken) => Tasks.FromResult(
                    CreateComputeOperationResponse("request001")));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.HostedServices.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request000",
                    StatusCode = HttpStatusCode.OK
                }));

            // Test
            client.RemoveCloudService(serviceName);

            // Assert
            clientMocks.ComputeManagementClientMock.Verify(
                c => c.Deployments.DeleteByNameAsync(serviceName, "myStagingdeployment", false, It.IsAny<CancellationToken>()), Times.Once);

            clientMocks.ComputeManagementClientMock.Verify(
                c => c.HostedServices.DeleteAsync(serviceName, It.IsAny<CancellationToken>()), Times.Once);

        }

        [TestMethod]
        public void TestRemoveCloudServiceWithoutDeployments()
        {
            RemoveDeployments();

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.BeginDeletingBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production, It.IsAny<CancellationToken>()))
                .Returns((string s, DeploymentSlot slot, CancellationToken cancellationToken) => Tasks.FromResult(new OperationResponse
                {
                    RequestId = "req0",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.HostedServices.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request000",
                    StatusCode = HttpStatusCode.OK
                }));

            // Test
            client.RemoveCloudService(serviceName);

            // Assert
            clientMocks.ComputeManagementClientMock.Verify(
                c => c.Deployments.BeginDeletingBySlotAsync(serviceName, DeploymentSlot.Production, It.IsAny<CancellationToken>()), Times.Never);

            clientMocks.ComputeManagementClientMock.Verify(
                c => c.HostedServices.DeleteAsync(serviceName, It.IsAny<CancellationToken>()), Times.Once);

        }

        [TestMethod]
        public void TestPublishNewCloudService()
        {
            RemoveDeployments();

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            using (var files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);


                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                clientMocks.ComputeManagementClientMock.Verify(c => c.Deployments.CreateAsync(
                    serviceName, DeploymentSlot.Production, It.IsAny<DeploymentCreateParameters>(), It.IsAny<CancellationToken>()), Times.Once);
            }

        }

        [TestMethod]
        public void TestUpgradeCloudService()
        {
            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.UpgradeBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentUpgradeParameters>(),
                                                 It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(CreateComputeOperationResponse("req002")));

            using (var files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                clientMocks.ComputeManagementClientMock.Verify(c => c.Deployments.UpgradeBySlotAsync(serviceName, DeploymentSlot.Production, It.IsAny<DeploymentUpgradeParameters>(), It.IsAny<CancellationToken>()), Times.Once);
            }

        }

        [TestMethod]
        public void TestCreateStorageServiceWithPublish()
        {
            RemoveDeployments();
            
            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            storageService.Clear();

            using (var files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                clientMocks.StorageManagementClientMock.Verify(c => c.StorageAccounts.CreateAsync(It.IsAny<StorageAccountCreateParameters>(), It.IsAny<CancellationToken>()), Times.Once);
            }            
        }

        [TestMethod]
        public void TestPublishWithCurrentStorageAccount()
        {
            RemoveDeployments();

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            using (var files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                subscription.CurrentStorageAccountName = storageName;

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                cloudBlobUtilityMock.Verify(f => f.UploadPackageToBlob(
                    clientMocks.StorageManagementClientMock.Object,
                    subscription.CurrentStorageAccountName,
                    It.IsAny<string>(),
                    It.IsAny<BlobRequestOptions>()), Times.Once());
            }           
        }

        [TestMethod]
        public void TestPublishWithDefaultLocation()
        {
            RemoveDeployments();

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new OperationResponse
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ManagementClientMock.Setup(c => c.Locations.ListAsync(It.IsAny<CancellationToken>()))
                .Returns(Tasks.FromResult(new LocationsListResponse
                {
                    Locations =
                    {
                        new LocationsListResponse.Location {DisplayName = "East US", Name = "EastUS"}
                    }
                }));

            using (var files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService());

                clientMocks.ManagementClientMock.Verify(c => c.Locations.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
            }            
        }

        private ComputeOperationStatusResponse CreateComputeOperationResponse(string requestId, OperationStatus status = OperationStatus.Succeeded)
        {
            return new ComputeOperationStatusResponse
            {
                Error = null,
                HttpStatusCode = HttpStatusCode.OK,
                Id = "id",
                RequestId = requestId,
                Status = status,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
