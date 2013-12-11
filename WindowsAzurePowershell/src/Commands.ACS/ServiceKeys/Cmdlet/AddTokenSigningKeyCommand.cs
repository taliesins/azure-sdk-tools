using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSTokenSigningKey", DefaultParameterSetName = "SymmetricKey")]
    public class AddTokenSigningKeyCommand : ValueReturningCommand<ServiceKey>
    {
        [Parameter(Mandatory = true, HelpMessage = "Token Signing Key object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public ServiceKey TokenSigningKey { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name for the service key", ParameterSetName = "SymmetricKey")]
        [Parameter(Mandatory = true, HelpMessage = "Name for the service key", ParameterSetName = "Certificate")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ParameterSetName = "SymmetricKey", Mandatory = true, HelpMessage = "Symmetric key")]
        [ValidateNotNull]
        public string Key { get; set; }

        [Parameter(ParameterSetName = "SymmetricKey", Mandatory = false, HelpMessage = "Effective UTC date for this symmetric key")]
        public DateTime EffectiveDate { get; set; }

        [Parameter(ParameterSetName = "SymmetricKey", Mandatory = false, HelpMessage = "Expiration UTC date for this symmetric key")]
        public DateTime ExpirationDate { get; set; }

        [Parameter(ParameterSetName = "Certificate", Mandatory = true, HelpMessage = "X.509 Certificate with a private key for token signing")]
        [ValidateNotNull]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Set this key as the primary token-signing key for the selected relying party application or service namespace", ParameterSetName = "SymmetricKey")]
        [Parameter(Mandatory = false, HelpMessage = "Set this key as the primary token-signing key for the selected relying party application or service namespace", ParameterSetName = "Certificate")]
        public SwitchParameter MakePrimary { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Relying party application that this certificate or key is used for", ParameterSetName = "SymmetricKey")]
        [Parameter(Mandatory = false, HelpMessage = "Relying party application that this certificate or key is used for", ParameterSetName = "Certificate")]
        [ValidateNotNullOrEmpty]
        public string RelyingPartyName { get; set; }

        public override ServiceKey ExecuteProcessRecordImplementation()
        {
            if (this.ParameterSetName.Equals("ObjectParamSet", StringComparison.OrdinalIgnoreCase))
            {
                this.ManagementService.AddServiceKey(this.TokenSigningKey.ToWrapperModel(), this.RelyingPartyName);
            }
            else
            {
                switch (this.ParameterSetName.ToUpperInvariant())
                {
                    case "SYMMETRICKEY":
                        this.ManagementService.AddServiceKey(this.Name, this.Key, string.Empty, KeyType.Symmetric, KeyUsage.Signing, this.EffectiveDate, this.ExpirationDate, this.MakePrimary.IsPresent, this.RelyingPartyName);
                        break;

                    case "CERTIFICATE":
                        this.ManagementService.AddServiceKey(this.Name, this.Certificate.RawData, string.Empty, KeyType.X509Certificate, KeyUsage.Signing, isPrimary: this.MakePrimary.IsPresent, relyingPartyName: this.RelyingPartyName);
                        break;
                }
            }

            return this.ManagementService.RetrieveServiceKey(this.Name).ToModel(this.ManagementService.MgmtSwtToken);
        }
    }
}
