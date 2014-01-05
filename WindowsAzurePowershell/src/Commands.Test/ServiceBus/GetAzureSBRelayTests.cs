using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceBus;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
using Moq;

namespace Microsoft.WindowsAzure.Commands.Test.ServiceBus
{
    [TestClass]
    public class GetAzureSBRelayTests : TestBase
    {
        Mock<ServiceBusClientExtensions> client;
        MockCommandRuntime mockCommandRuntime;
        GetAzureSBRelayCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            client = new Mock<ServiceBusClientExtensions>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureSBRelayCommand()
            {
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
        }

        [TestMethod]
        public void GetAzureSBRelaySuccessfull()
        {
            // Setup
            string path = "test";
            RelayType relayType = RelayType.Http;
            string nameSpace = "test";

            cmdlet.Name = path;
            cmdlet.Namespace = nameSpace;

            RelayDescription expected = new RelayDescription(path, relayType);
            client.Setup(f => f.GetRelay(nameSpace, path)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            RelayDescription actual = mockCommandRuntime.OutputPipeline[0] as RelayDescription;

            Assert.AreEqual<string>(expected.CollectionName, actual.CollectionName);
            Assert.AreEqual<string>(expected.Path, actual.Path);
            Assert.AreEqual<AuthorizationRules>(expected.Authorization, actual.Authorization);
            Assert.AreEqual<DateTime>(expected.CreatedAt, actual.CreatedAt);
            Assert.AreEqual<ExtensionDataObject>(expected.ExtensionData, actual.ExtensionData);
            Assert.AreEqual<bool>(expected.IsDynamic, actual.IsDynamic);
            Assert.AreEqual<bool>(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual<int>(expected.ListenerCount, actual.ListenerCount);
            Assert.AreEqual<RelayType>(expected.RelayType, actual.RelayType);
            Assert.AreEqual<bool>(expected.RequiresClientAuthorization, actual.RequiresClientAuthorization);
            Assert.AreEqual<bool>(expected.RequiresTransportSecurity, actual.RequiresTransportSecurity);
            Assert.AreEqual<DateTime>(expected.UpdatedAt, actual.UpdatedAt);
        }

        [TestMethod]
        public void ListAzureSBRelaysSuccessfull()
        {
            // Setup
            string path = "test";
            RelayType relayType = RelayType.Http;
            string nameSpace = "test";

            cmdlet.Namespace = nameSpace;

            List<RelayDescription> expected = new List<RelayDescription>();
            expected.Add(new RelayDescription(path, relayType));
            client.Setup(f => f.GetRelay(nameSpace)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<RelayDescription> actual = mockCommandRuntime.OutputPipeline[0] as List<RelayDescription>;
            Assert.AreEqual<int>(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual<string>(expected[i].CollectionName, actual[i].CollectionName);
                Assert.AreEqual<string>(expected[i].Path, actual[i].Path);
                Assert.AreEqual<AuthorizationRules>(expected[i].Authorization, actual[i].Authorization);
                Assert.AreEqual<DateTime>(expected[i].CreatedAt, actual[i].CreatedAt);
                Assert.AreEqual<ExtensionDataObject>(expected[i].ExtensionData, actual[i].ExtensionData);
                Assert.AreEqual<bool>(expected[i].IsDynamic, actual[i].IsDynamic);
                Assert.AreEqual<bool>(expected[i].IsReadOnly, actual[i].IsReadOnly);
                Assert.AreEqual<int>(expected[i].ListenerCount, actual[i].ListenerCount);
                Assert.AreEqual<RelayType>(expected[i].RelayType, actual[i].RelayType);
                Assert.AreEqual<bool>(expected[i].RequiresClientAuthorization, actual[i].RequiresClientAuthorization);
                Assert.AreEqual<bool>(expected[i].RequiresTransportSecurity, actual[i].RequiresTransportSecurity);
                Assert.AreEqual<DateTime>(expected[i].UpdatedAt, actual[i].UpdatedAt);
            }
        }
    }
}
