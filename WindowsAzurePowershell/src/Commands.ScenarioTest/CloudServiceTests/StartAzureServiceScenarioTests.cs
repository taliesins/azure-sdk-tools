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
    public class StartAzureServiceScenarioTests : WindowsAzurePowerShellTest
    {
        public StartAzureServiceScenarioTests()
            : base("CloudService\\Common.ps1",
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
        public void TestStartAzureServiceWithInvalidCredentials()
        {
            RunPowerShellTest("Test-WithInvalidCredentials { Start-AzureService $(Get-HostedService) }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestStartAzureServiceWithNonExistingService()
        {
            RunPowerShellTest("Test-StartAzureServiceWithNonExistingService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        [TestCategory(Category.OneSDK)]
        [TestCategory(Category.CIT)]
        public void TestStartAzureServiceWithProductionDeployment()
        {
            RunPowerShellTest("Test-StartAzureServiceWithProductionDeployment");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestStartAzureServiceWithStagingDeployment()
        {
            RunPowerShellTest("Test-StartAzureServiceWithStagingDeployment");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestStartAzureServiceWithEmptyDeployment()
        {
            RunPowerShellTest("Test-StartAzureServiceWithEmptyDeployment");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestStartAzureServiceWithoutName()
        {
            RunPowerShellTest("Test-StartAzureServiceWithoutName");
        }
    }
}
