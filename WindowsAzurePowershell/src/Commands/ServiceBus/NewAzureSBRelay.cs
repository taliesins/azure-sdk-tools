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

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus;

    /// <summary>
    /// Creates new service bus relay.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSBRelay"), OutputType(typeof(RelayDescription))]
    public class NewAzureSBRelay : CmdletWithSubscriptionBase
    {
        internal ServiceBusClientExtensions Client { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The relay name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The relay type")]
        public RelayType RelayType { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The namespace name")]
        public string Namespace { get; set; }

        /// <summary>
        /// Creates a new relay.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);

            var notificationHubDescription = new RelayDescription(this.Name, this.RelayType);

            WriteObject(Client.CreateRelay(Namespace, notificationHubDescription));
        }
    }
}