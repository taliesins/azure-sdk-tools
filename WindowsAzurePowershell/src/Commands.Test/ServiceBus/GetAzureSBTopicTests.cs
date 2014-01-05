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
    public class GetAzureSBTopicTests : TestBase
    {
        Mock<ServiceBusClientExtensions> client;
        MockCommandRuntime mockCommandRuntime;
        GetAzureSBTopicCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            client = new Mock<ServiceBusClientExtensions>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new GetAzureSBTopicCommand()
            {
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
        }

        [TestMethod]
        public void GetAzureSBTopicSuccessfull()
        {
            // Setup
            string path = "test";
            string nameSpace = "test";

            cmdlet.Name = path;
            cmdlet.Namespace = nameSpace;

            TopicDescription expected = new TopicDescription(path);
            client.Setup(f => f.GetTopic(nameSpace, path)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            TopicDescription actual = mockCommandRuntime.OutputPipeline[0] as TopicDescription;

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
            Assert.AreEqual<bool>(expected.EnableFilteringMessagesBeforePublishing, actual.EnableFilteringMessagesBeforePublishing);
            Assert.AreEqual<ExtensionDataObject>(expected.ExtensionData, actual.ExtensionData);
            Assert.AreEqual<bool>(expected.IsAnonymousAccessible, actual.IsAnonymousAccessible);
            Assert.AreEqual<bool>(expected.IsReadOnly, actual.IsReadOnly);
            Assert.AreEqual<long>(expected.MaxSizeInMegabytes, actual.MaxSizeInMegabytes);
            Assert.AreEqual<MessageCountDetails>(expected.MessageCountDetails, actual.MessageCountDetails);
            Assert.AreEqual<bool>(expected.RequiresDuplicateDetection, actual.RequiresDuplicateDetection);
            Assert.AreEqual<long>(expected.SizeInBytes, actual.SizeInBytes);
            Assert.AreEqual<EntityStatus>(expected.Status, actual.Status);
            Assert.AreEqual<int>(expected.SubscriptionCount, actual.SubscriptionCount);
            Assert.AreEqual<bool>(expected.SupportOrdering, actual.SupportOrdering);
            Assert.AreEqual<DateTime>(expected.UpdatedAt, actual.UpdatedAt);   
        }

        [TestMethod]
        public void ListAzureSBTopicsSuccessfull()
        {
            // Setup
            string path = "test";
            string nameSpace = "test";

            cmdlet.Namespace = nameSpace;

            List<TopicDescription> expected = new List<TopicDescription>();
            expected.Add(new TopicDescription(path));
            client.Setup(f => f.GetTopic(nameSpace)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<TopicDescription> actual = mockCommandRuntime.OutputPipeline[0] as List<TopicDescription>;
            Assert.AreEqual<int>(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
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
                Assert.AreEqual<bool>(expected[i].EnableFilteringMessagesBeforePublishing, actual[i].EnableFilteringMessagesBeforePublishing);
                Assert.AreEqual<ExtensionDataObject>(expected[i].ExtensionData, actual[i].ExtensionData);
                Assert.AreEqual<bool>(expected[i].IsAnonymousAccessible, actual[i].IsAnonymousAccessible);
                Assert.AreEqual<bool>(expected[i].IsReadOnly, actual[i].IsReadOnly);
                Assert.AreEqual<long>(expected[i].MaxSizeInMegabytes, actual[i].MaxSizeInMegabytes);
                Assert.AreEqual<MessageCountDetails>(expected[i].MessageCountDetails, actual[i].MessageCountDetails);
                Assert.AreEqual<bool>(expected[i].RequiresDuplicateDetection, actual[i].RequiresDuplicateDetection);
                Assert.AreEqual<long>(expected[i].SizeInBytes, actual[i].SizeInBytes);
                Assert.AreEqual<EntityStatus>(expected[i].Status, actual[i].Status);
                Assert.AreEqual<int>(expected[i].SubscriptionCount, actual[i].SubscriptionCount);
                Assert.AreEqual<bool>(expected[i].SupportOrdering, actual[i].SupportOrdering);
                Assert.AreEqual<DateTime>(expected[i].UpdatedAt, actual[i].UpdatedAt);               
            }
        }
    }
}
