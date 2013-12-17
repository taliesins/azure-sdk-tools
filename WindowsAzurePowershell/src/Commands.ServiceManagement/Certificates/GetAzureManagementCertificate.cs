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

using System.Threading;
using Microsoft.WindowsAzure.Management.Models;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Certificates
{
    using System.Linq;
    using System.Management.Automation;
    using Model;
    using Utilities.Common;

    /// <summary>
    /// Retrieve a specified service certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureManagementCertificate"), OutputType(typeof(CertificateContext))]
    public class GetAzureManagementCertificate : ServiceManagementBaseCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Certificate thumbprint.")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (this.Thumbprint != null)
            {
                ExecuteClientActionNewSM(
                    "Get azure management certificate",
                    CommandRuntime.ToString(),
                    () => this.ManagementClient.ManagementCertificates.GetAsync(Thumbprint, new CancellationToken()).Result,
                    (s, response) => new int[1].Select(i => ContextFactory<ManagementCertificateGetResponse, ManagementCertificateContext>(response, s)));
            }
            else
            {
                ExecuteClientActionNewSM(
                    "Get azure management certificates",
                    CommandRuntime.ToString(),
                    () => this.ManagementClient.ManagementCertificates.ListAsync(new CancellationToken()).Result,
                    (s, response) => response.SubscriptionCertificates.Select(c =>
                                                                  {
                                                                      var context = ContextFactory<ManagementCertificateListResponse.SubscriptionCertificate, ManagementCertificateContext>(c, s);
                                                                      return context;
                                                                  }));
            }
        }

        public void ExecuteCommand()
        {
            OnProcessRecord();
        }
    }
}