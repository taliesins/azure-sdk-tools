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
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using Utilities.Common;

    /// <summary>
    /// Upload a service certificate for the specified hosted service.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureManagementCertificate"), OutputType(typeof(ManagementOperationContext))]
    public class AddAzureManagementCertificate : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Certificate to deploy.")]
        [ValidateNotNullOrEmpty]
        public object CertToDeploy
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Certificate password.")]
        public string Password
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            Password = Password ?? string.Empty;

            var certificate = GetCertificate();

            var parameters = new ManagementCertificateCreateParameters
            {
                PublicKey = certificate.GetPublicKey(),
                Thumbprint = certificate.Thumbprint,
                Data = certificate.RawData
            };
            ExecuteClientActionNewSM(
                "Add azure management certificate", 
                CommandRuntime.ToString(),
                () => this.ManagementClient.ManagementCertificates.CreateAsync(parameters, new CancellationToken()).Result);
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            this.ExecuteCommand();
        }

        private X509Certificate2 GetCertificate()
        {
            if ((CertToDeploy is PSObject) && ((PSObject)CertToDeploy).ImmediateBaseObject is X509Certificate2)
            {
                var cert = ((PSObject)CertToDeploy).ImmediateBaseObject as X509Certificate2;
                return cert;
            }
            else if (CertToDeploy is X509Certificate2)
            {
                return CertToDeploy as X509Certificate2;
            }
            else
            {
                var certPath = this.ResolvePath(CertToDeploy.ToString());
                var cert = new X509Certificate2();
                cert.Import(certPath, Password, X509KeyStorageFlags.Exportable);
                return cert;
            }
        }
    }
}
