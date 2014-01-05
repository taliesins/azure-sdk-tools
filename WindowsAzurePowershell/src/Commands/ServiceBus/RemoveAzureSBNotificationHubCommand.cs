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

namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus;

    /// <summary>
    /// Creates new service bus notification hub.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureSBNotificationHub", SupportsShouldProcess = true), OutputType(typeof(bool))]
    public class RemoveAzureSBNotificationHubCommand : CmdletWithSubscriptionBase
    {
        public ServiceBusClientExtensions Client { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The notification hub name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The namespace name")]
        public string Namespace { get; set; }

        [Parameter(Position = 2, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 3, HelpMessage = "Do not confirm the removal of the notification hub")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Removes a service bus notification hub.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            ConfirmAction(
                Force.IsPresent,
                string.Format("Are you sure you want to delete the notification hub '{0}'?", Name),
                string.Format("Deleting notification hub"),
                Name,
                () =>
                {
                    Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);
                    Client.RemoveNotificationHub(Namespace, Name);

                    if (PassThru)
                    {
                        WriteObject(true);
                    }
                });
        }
    }
}