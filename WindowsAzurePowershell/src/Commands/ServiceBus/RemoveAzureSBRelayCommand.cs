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

namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus;

    /// <summary>
    /// Creates new service bus relay.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureSBRelay", SupportsShouldProcess = true), OutputType(typeof(bool))]
    public class RemoveAzureSBRelayCommand : CmdletWithSubscriptionBase
    {
        public ServiceBusClientExtensions Client { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The relay name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The namespace name")]
        public string Namespace { get; set; }

        [Parameter(Position = 2, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 3, HelpMessage = "Do not confirm the removal of the relay")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Removes a service bus relay.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            ConfirmAction(
                Force.IsPresent,
                string.Format("Are you sure you want to delete the relay '{0}'?", Name),
                string.Format("Deleting relay"),
                Name,
                () =>
                {
                    Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);
                    Client.RemoveRelay(Namespace, Name);

                    if (PassThru)
                    {
                        WriteObject(true);
                    }
                });
        }
    }
}