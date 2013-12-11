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
    /// Creates new service bus queue.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSBQueue"), OutputType(typeof(QueueDescription))]
    public class NewAzureSBQueue : CmdletWithSubscriptionBase
    {
        public const string EntitySASParameterSet = "EntitySAS";
        public const string NamespaceSASParameterSet = "NamespaceSAS";

        internal ServiceBusClientExtensions Client { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = EntitySASParameterSet, ValueFromPipelineByPropertyName = true, HelpMessage = "The queue name")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = NamespaceSASParameterSet, ValueFromPipelineByPropertyName = true, HelpMessage = "The queue name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
        ParameterSetName = EntitySASParameterSet, HelpMessage = "The namespace name")]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
        ParameterSetName = NamespaceSASParameterSet, HelpMessage = "The namespace name")]
        public string Namespace { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Message time to live. defaults to 30 days")]
        public TimeSpan DefaultMessageTimeToLive { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Duplicate detection history time windows. defaults to 60 seconds")]
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Lock duration, defaults to 30 seconds")]
        public TimeSpan LockDuration { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "How many times we should retry sending a message to the queue. Defaults to 1")]
        public int MaxDeliveryCount { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Maximum queue size in megabytes. Defaults to 1024")]
        public int MaxSizeInMegabytes { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Requires duplicate detection. Defaults to false")]
        public bool RequiresDuplicateDetection { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Requires detection. Defaults to false")]
        public bool RequiresSession { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Enable dead lettering on message expiration. Defaults to false")]
        public bool EnableDeadLetteringOnMessageExpiration { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Enable batch operations. Defaults to false")]
        public bool EnableBatchedOperations { get; set; }

        public NewAzureSBQueue()
        {
            this.DefaultMessageTimeToLive = TimeSpan.FromDays(30);
            this.DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(60);
            this.LockDuration = TimeSpan.FromSeconds(30);
            this.MaxDeliveryCount = 1;
            this.MaxSizeInMegabytes = 1024;
            this.RequiresDuplicateDetection = false;
            this.RequiresSession = false;
            this.EnableDeadLetteringOnMessageExpiration = false;
            this.EnableBatchedOperations = false;
        }

        /// <summary>
        /// Creates a new service bus queue.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);

            var queueDescription = new QueueDescription(this.Name)
            {
                DefaultMessageTimeToLive = this.DefaultMessageTimeToLive,
                DuplicateDetectionHistoryTimeWindow = this.DuplicateDetectionHistoryTimeWindow,
                LockDuration = this.LockDuration,
                MaxSizeInMegabytes = this.MaxSizeInMegabytes,
                MaxDeliveryCount = this.MaxDeliveryCount,
                RequiresDuplicateDetection = this.RequiresDuplicateDetection,
                RequiresSession = this.RequiresSession,
                EnableDeadLetteringOnMessageExpiration = this.EnableDeadLetteringOnMessageExpiration,
                EnableBatchedOperations = this.EnableBatchedOperations
            };

            WriteObject(Client.CreateQueue(Namespace, queueDescription));
        }
    }
}