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

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.CloudServiceTests
{
    using Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestAzureNameScenarioTests : WindowsAzurePowerShellTest
    {
        public TestAzureNameScenarioTests()
            : base("CloudService\\Common.ps1",
                   "ServiceBus\\Common.ps1",
                   "CloudService\\CloudServiceTests.ps1")
        {

        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            powershell.AddScript("Initialize-CloudServiceTest");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithInvalidCredentials()
        {
            RunPowerShellTest("Test-WithInvalidCredentials { Test-AzureName -Service $(Get-HostedService) }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithNotExistingHostedService()
        {
            RunPowerShellTest("Test-AzureNameWithNotExistingHostedService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithExistingHostedService()
        {
            RunPowerShellTest("Test-AzureNameWithExistingHostedService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithInvalidHostedService()
        {
            RunPowerShellTest("Test-AzureNameWithInvalidHostedService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithNotExistingStorageService()
        {
            RunPowerShellTest("Test-AzureNameWithNotExistingStorageService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithExistingStorageService()
        {
            RunPowerShellTest("Test-AzureNameWithExistingStorageService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithInvalidStorageService()
        {
            RunPowerShellTest("Test-AzureNameWithInvalidStorageService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithNotExistingServiceBusNamespace()
        {
            RunPowerShellTest("Test-AzureNameWithNotExistingServiceBusNamespace");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestAzureNameWithExistingServiceBusNamespace()
        {
            RunPowerShellTest("Test-AzureNameWithExistingServiceBusNamespace");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        [Ignore] // https://github.com/WindowsAzure/azure-sdk-tools/issues/1185
        public void TestAzureNameWithInvalidServiceBusNamespace()
        {
            RunPowerShellTest("Test-AzureNameWithInvalidServiceBusNamespace");
        }
    }
}
