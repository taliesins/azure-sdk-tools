using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceBus;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
using Moq;

namespace Microsoft.WindowsAzure.Commands.Test.ServiceBus
{
    [TestClass]
    public class GetAzureSBNotificationHubTests : TestBase
    {
        Mock<ServiceBusClientExtensions> client;
        MockCommandRuntime mockCommandRuntime;
        GetAzureSBNotificationHubCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            client = new Mock<ServiceBusClientExtensions>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureSBNotificationHubCommand()
            {
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
        }

        [TestMethod]
        public void GetAzureSBNotificationHubSuccessfull()
        {
            // Setup
            string path = "test";
            string nameSpace = "test";

            cmdlet.Name = path;
            cmdlet.Namespace = nameSpace;

            NotificationHubDescription expected = new NotificationHubDescription(path);
            client.Setup(f => f.GetNotificationHub(nameSpace, path)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            NotificationHubDescription actual = mockCommandRuntime.OutputPipeline[0] as NotificationHubDescription;
            
            Assert.AreEqual<string>(expected.Path, actual.Path);
            Assert.AreEqual<string>(expected.UserMetadata, actual.UserMetadata);
            Assert.AreEqual<ApnsCredential>(expected.ApnsCredential, actual.ApnsCredential);
            Assert.AreEqual<AuthorizationRules>(expected.Authorization, actual.Authorization);
            Assert.AreEqual<long>(expected.DailyMaxActiveDevices, actual.DailyMaxActiveDevices);
            Assert.AreEqual<long>(expected.DailyMaxActiveRegistrations, actual.DailyMaxActiveRegistrations);
            Assert.AreEqual<long>(expected.DailyOperations, actual.DailyOperations);
            Assert.AreEqual<GcmCredential>(expected.GcmCredential, actual.GcmCredential);
            Assert.AreEqual<bool>(expected.IsAnonymousAccessible, actual.IsAnonymousAccessible);
            Assert.AreEqual<MpnsCredential>(expected.MpnsCredential, actual.MpnsCredential);
            Assert.AreEqual<Nullable<TimeSpan>>(expected.RegistrationTtl, actual.RegistrationTtl);
            Assert.AreEqual<WnsCredential>(expected.WnsCredential, actual.WnsCredential);
        }

        [TestMethod]
        public void ListAzureSBNotificationHubsSuccessfull() 
        {
            // Setup
            string path = "test";
            string nameSpace = "test";

            cmdlet.Namespace = nameSpace;

            List<NotificationHubDescription> expected = new List<NotificationHubDescription>();
            expected.Add(new NotificationHubDescription(path));
            client.Setup(f => f.GetNotificationHub(nameSpace)).Returns(expected);
            
            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<NotificationHubDescription> actual = mockCommandRuntime.OutputPipeline[0] as List<NotificationHubDescription>;
            Assert.AreEqual<int>(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual<string>(expected[i].Path, actual[i].Path);
                Assert.AreEqual<string>(expected[i].UserMetadata, actual[i].UserMetadata);
                Assert.AreEqual<ApnsCredential>(expected[i].ApnsCredential, actual[i].ApnsCredential);
                Assert.AreEqual<AuthorizationRules>(expected[i].Authorization, actual[i].Authorization);
                Assert.AreEqual<long>(expected[i].DailyMaxActiveDevices, actual[i].DailyMaxActiveDevices);
                Assert.AreEqual<long>(expected[i].DailyMaxActiveRegistrations, actual[i].DailyMaxActiveRegistrations);
                Assert.AreEqual<long>(expected[i].DailyOperations, actual[i].DailyOperations);
                Assert.AreEqual<GcmCredential>(expected[i].GcmCredential, actual[i].GcmCredential);
                Assert.AreEqual<bool>(expected[i].IsAnonymousAccessible, actual[i].IsAnonymousAccessible);
                Assert.AreEqual<MpnsCredential>(expected[i].MpnsCredential, actual[i].MpnsCredential);
                Assert.AreEqual<Nullable<TimeSpan>>(expected[i].RegistrationTtl, actual[i].RegistrationTtl);
                Assert.AreEqual<WnsCredential>(expected[i].WnsCredential, actual[i].WnsCredential);

            }
        }
    }
}
