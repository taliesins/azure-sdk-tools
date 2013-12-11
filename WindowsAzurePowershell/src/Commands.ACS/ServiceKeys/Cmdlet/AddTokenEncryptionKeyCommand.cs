using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSTokenEncryptionKey", DefaultParameterSetName = "PropertiesParamSet")]
    public class AddTokenEncryptionKeyCommand : ValueReturningCommand<ServiceKey>
    {
        [Parameter(Mandatory = true, HelpMessage = "Service Key object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public ServiceKey TokenEncryptionKey { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name for the service key", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "X.509 Certificate for token encryption", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The relying party application that this encryption certificate will be used for")]
        public string RelyingPartyName { get; set; }

        public override ServiceKey ExecuteProcessRecordImplementation()
        {
            if (this.ParameterSetName.Equals("ObjectParamSet", StringComparison.OrdinalIgnoreCase))
            {
                this.ManagementService.AddServiceKey(this.TokenEncryptionKey.ToWrapperModel(), this.RelyingPartyName);
            }
            else
            {
                this.ManagementService.AddServiceKey(
                    this.Name,
                    this.Certificate.RawData,
                    string.Empty,
                    KeyType.X509Certificate,
                    KeyUsage.Encrypting,
                    relyingPartyName: this.RelyingPartyName);
            }

            return this.ManagementService.RetrieveServiceKey(this.Name).ToModel(this.ManagementService.MgmtSwtToken);
        }
    }
}
