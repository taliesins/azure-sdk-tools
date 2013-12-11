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
    using System.Collections.Generic;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus;

    /// <summary>
    /// Lists all service bus relays
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSBRelay"), OutputType(typeof(List<RelayDescription>), typeof(RelayDescription))]
    public class GetAzureSBRelay : CmdletWithSubscriptionBase
    {
        public const string EntitySASParameterSet = "EntitySAS";
        public const string NamespaceSASParameterSet = "NamespaceSAS";

        internal ServiceBusClientExtensions Client { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = EntitySASParameterSet, ValueFromPipelineByPropertyName = true, HelpMessage = "The relay name")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = NamespaceSASParameterSet, ValueFromPipelineByPropertyName = true, HelpMessage = "The relay name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = EntitySASParameterSet, HelpMessage = "The namespace name")]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = NamespaceSASParameterSet, HelpMessage = "The namespace name")]
        public string Namespace { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);

            if (string.IsNullOrEmpty(Name))
            {
                WriteObject(Client.GetRelay(Namespace), true);
            }
            else
            {
                WriteObject(Client.GetRelay(Namespace, Name));
            }
        }
    }
}