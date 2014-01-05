// ----------------------------------------------------------------------------------
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

using Microsoft.ServiceBus.Messaging;

namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using System;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus;

    /// <summary>
    /// Creates new service bus topic.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSBTopic"), OutputType(typeof(TopicDescription))]
    public class NewAzureSBTopicCommand : CmdletWithSubscriptionBase
    {
        internal ServiceBusClientExtensions Client { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The topic name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The namespace name")]
        public string Namespace { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Message time to live. defaults to 30 days")]
        public TimeSpan DefaultMessageTimeToLive { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Duplicate detection history time windows. defaults to 60 seconds")]
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Maximum queue size in megabytes. Defaults to 1024")]
        public int MaxSizeInMegabytes { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Requires duplicate detection. Defaults to false")]
        public bool RequiresDuplicateDetection { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Enable batch operations. Defaults to false")]
        public bool EnableBatchedOperations { get; set; }

        public NewAzureSBTopicCommand()
        {
            this.DefaultMessageTimeToLive = TimeSpan.FromDays(30);
            this.DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(60);
            this.MaxSizeInMegabytes = 1024;
            this.RequiresDuplicateDetection = false;
            this.EnableBatchedOperations = false;
        }

        /// <summary>
        /// Creates a new service bus topic.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);

            var topicDescription = new TopicDescription(this.Name)
            {
                DefaultMessageTimeToLive = this.DefaultMessageTimeToLive,
                DuplicateDetectionHistoryTimeWindow = this.DuplicateDetectionHistoryTimeWindow,
                MaxSizeInMegabytes = this.MaxSizeInMegabytes,
                RequiresDuplicateDetection = this.RequiresDuplicateDetection,
                EnableBatchedOperations = this.EnableBatchedOperations,
            };

            WriteObject(Client.CreateTopic(Namespace, topicDescription));
        }
    }
}