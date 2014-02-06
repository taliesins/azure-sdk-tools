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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System.Management.Automation;
    using Management.VirtualNetworks;
    using Model;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Get, "AzureVNetGatewayKey"), OutputType(typeof(SharedKeyContext))]
    public class GetAzureVNetGatewayKeyCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "The virtual network name.")]
        [ValidateNotNullOrEmpty]
        public string VNetName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "The local network site name.")]
        [ValidateNotNullOrEmpty]
        public string LocalNetworkSiteName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteClientActionNewSM(
                null,
                this.CommandRuntime.ToString(),
                () => this.NetworkClient.Gateways.GetSharedKey(this.VNetName, this.LocalNetworkSiteName),
                (operation, sharedKey) => new SharedKeyContext
                {
                    OperationId          = operation.Id,
                    OperationDescription = this.CommandRuntime.ToString(),
                    OperationStatus      = operation.Status.ToString(),
                    Value                = sharedKey.SharedKey
                });
        }
    }
}
