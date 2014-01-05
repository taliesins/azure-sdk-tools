using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceBus;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
using Moq;

namespace Microsoft.WindowsAzure.Commands.Test.ServiceBus
{
    [TestClass]
    public class GetAzureSBQueueTests : TestBase
    {
        Mock<ServiceBusClientExtensions> client;
        MockCommandRuntime mockCommandRuntime;
        GetAzureSBQueueCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            client = new Mock<ServiceBusClientExtensions>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureSBQueueCommand()
            {
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
        }

        [TestMethod]
        public void GetAzureSBQueueSuccessfull()
        {
            // Setup
            string path = "test";
            string nameSpace = "test";

            cmdlet.Name = path;
            cmdlet.Namespace = nameSpace;
            QueueDescription expected = new QueueDescription(path);
            client.Setup(f => f.GetQueue(nameSpace, path)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            QueueDescription actual = mockCommandRuntime.OutputPipeline[0] as QueueDescription;

            Assert.AreEqual<string>(expected.ForwardTo, actual.ForwardTo);
            Assert.AreEqual<string>(expected.Path, actual.Path);
            Assert.AreEqual<string>(expected.UserMetadata, actual.UserMetadata);
            Assert.AreEqual<DateTime>(expected.AccessedAt, actual.AccessedAt);
            Assert.AreEqual<AuthorizationRules>(expected.Authorization, actual.Authorization);
            Assert.AreEqual<TimeSpan>(expected.AutoDeleteOnIdle, actual.AutoDeleteOnIdle);
            Assert.AreEqual<EntityAvailabilityStatus>(expected.AvailabilityStatus, actual.AvailabilityStatus);
            Assert.AreEqual<DateTime>(expected.CreatedAt, actual.CreatedAt);
            Assert.AreEqual<TimeSpan>(expected.DefaultMessageTimeToLive, actual.DefaultMessageTimeToLive);
            Assert.AreEqual<TimeSpan>(expected.DuplicateDetectionHistoryTimeWindow, actual.DuplicateDetectionHistoryTimeWindow);
            Assert.AreEqual<bool>(expected.EnableBatchedOperations, actual.EnableBatchedOperations);
            Assert.AreEqual<bool>(expected.EnableDeadLetteringOnMessageExpiration, actual.EnableDeadLetteringOnMessageExpiration);
            Assert.AreEqual<ExtensionDataObject>(expected.ExtensionData, actual.ExtensionData);
            Assert.AreEqual<bool>(expected.IsAnonymousAccessible, actual.IsAnonymousAccessible);
            Assert.AreEqual<bool>(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual<TimeSpan>(expected.LockDuration, actual.LockDuration);
            Assert.AreEqual<int>(expected.MaxDeliveryCount, actual.MaxDeliveryCount);
            Assert.AreEqual<long>(expected.MaxSizeInMegabytes, actual.MaxSizeInMegabytes);
            Assert.AreEqual<long>(expected.MessageCount, actual.MessageCount);
            Assert.AreEqual<MessageCountDetails>(expected.MessageCountDetails, actual.MessageCountDetails);
            Assert.AreEqual<bool>(expected.RequiresDuplicateDetection, actual.RequiresDuplicateDetection);
            Assert.AreEqual<bool>(expected.RequiresSession, actual.RequiresSession);
            Assert.AreEqual<long>(expected.SizeInBytes, actual.SizeInBytes);
            Assert.AreEqual<EntityStatus>(expected.Status, actual.Status);
            Assert.AreEqual<bool>(expected.SupportOrdering, actual.SupportOrdering);
            Assert.AreEqual<DateTime>(expected.UpdatedAt, actual.UpdatedAt);
        }

        [TestMethod]
        public void ListAzureSBQueuesSuccessfull()
        {
            // Setup
            string path = "test";
            string nameSpace = "test";

            cmdlet.Namespace = nameSpace;

            List<QueueDescription> expected = new List<QueueDescription>();
            expected.Add(new QueueDescription(path));
            client.Setup(f => f.GetQueue(nameSpace)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<QueueDescription> actual = mockCommandRuntime.OutputPipeline[0] as List<QueueDescription>;
            Assert.AreEqual<int>(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual<string>(expected[i].ForwardTo, actual[i].ForwardTo);
                Assert.AreEqual<string>(expected[i].Path, actual[i].Path);
                Assert.AreEqual<string>(expected[i].UserMetadata, actual[i].UserMetadata);
                Assert.AreEqual<DateTime>(expected[i].AccessedAt, actual[i].AccessedAt);
                Assert.AreEqual<AuthorizationRules>(expected[i].Authorization, actual[i].Authorization);
                Assert.AreEqual<TimeSpan>(expected[i].AutoDeleteOnIdle, actual[i].AutoDeleteOnIdle);
                Assert.AreEqual<EntityAvailabilityStatus>(expected[i].AvailabilityStatus, actual[i].AvailabilityStatus);
                Assert.AreEqual<DateTime>(expected[i].CreatedAt, actual[i].CreatedAt);
                Assert.AreEqual<TimeSpan>(expected[i].DefaultMessageTimeToLive, actual[i].DefaultMessageTimeToLive);
                Assert.AreEqual<TimeSpan>(expected[i].DuplicateDetectionHistoryTimeWindow, actual[i].DuplicateDetectionHistoryTimeWindow);
                Assert.AreEqual<bool>(expected[i].EnableBatchedOperations, actual[i].EnableBatchedOperations);
                Assert.AreEqual<bool>(expected[i].EnableDeadLetteringOnMessageExpiration, actual[i].EnableDeadLetteringOnMessageExpiration);
                Assert.AreEqual<ExtensionDataObject>(expected[i].ExtensionData, actual[i].ExtensionData);
                Assert.AreEqual<bool>(expected[i].IsAnonymousAccessible, actual[i].IsAnonymousAccessible);
                Assert.AreEqual<bool>(expected[i].IsReadOnly, actual[i].IsReadOnly);
                Assert.AreEqual<TimeSpan>(expected[i].LockDuration, actual[i].LockDuration);
                Assert.AreEqual<int>(expected[i].MaxDeliveryCount, actual[i].MaxDeliveryCount);
                Assert.AreEqual<long>(expected[i].MaxSizeInMegabytes, actual[i].MaxSizeInMegabytes);
                Assert.AreEqual<long>(expected[i].MessageCount, actual[i].MessageCount);
                Assert.AreEqual<MessageCountDetails>(expected[i].MessageCountDetails, actual[i].MessageCountDetails);
                Assert.AreEqual<bool>(expected[i].RequiresDuplicateDetection, actual[i].RequiresDuplicateDetection);
                Assert.AreEqual<bool>(expected[i].RequiresSession, actual[i].RequiresSession);
                Assert.AreEqual<long>(expected[i].SizeInBytes, actual[i].SizeInBytes);
                Assert.AreEqual<EntityStatus>(expected[i].Status, actual[i].Status);
                Assert.AreEqual<bool>(expected[i].SupportOrdering, actual[i].SupportOrdering);
                Assert.AreEqual<DateTime>(expected[i].UpdatedAt, actual[i].UpdatedAt);
            }
        }
    }
}
