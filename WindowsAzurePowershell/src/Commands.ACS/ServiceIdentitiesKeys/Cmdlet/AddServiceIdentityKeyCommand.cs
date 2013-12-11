using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceIdentitiesKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSServiceIdentityKey", DefaultParameterSetName = "SymmetricKey")]
    public class AddServiceIdentityKeyCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, HelpMessage = "Service Identity Key object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public ServiceIdentityKey ServiceIdentityKey { get; set; }

        [Parameter(ParameterSetName = "SymmetricKey", Mandatory = true, HelpMessage = "Symmetric key for the Service Identity")]
        [ValidateNotNullOrEmpty]
        public string Key { get; set; }

        [Parameter(ParameterSetName = "Password", Mandatory = true, HelpMessage = "Password for the Service Identity")]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        [Parameter(ParameterSetName = "Certificate", Mandatory = true, HelpMessage = "X.509 Certificate for the Service Identity")]
        [ValidateNotNull]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(ParameterSetName = "SymmetricKey", Mandatory = false, HelpMessage = "Effective UTC date for this key")]
        [Parameter(ParameterSetName = "Password", Mandatory = false, HelpMessage = "Effective UTC date for this key")]
        public DateTime EffectiveDate { get; set; }

        [Parameter(ParameterSetName = "SymmetricKey", Mandatory = false, HelpMessage = "Expiration UTC date for this key")]
        [Parameter(ParameterSetName = "Password", Mandatory = false, HelpMessage = "Expiration UTC date for this key")]
        public DateTime ExpirationDate { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Service Identity that this certificate or key is used for", ParameterSetName = "SymmetricKey")]
        [Parameter(Mandatory = true, HelpMessage = "Service Identity that this certificate or key is used for", ParameterSetName = "Certificate")]
        [Parameter(Mandatory = true, HelpMessage = "Service Identity that this certificate or key is used for", ParameterSetName = "Password")]
        [Parameter(Mandatory = true, HelpMessage = "Service Identity that this certificate or key is used for", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public string ServiceIdentityName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Name for the Service Identity Key", ParameterSetName = "SymmetricKey")]
        [Parameter(Mandatory = false, HelpMessage = "Name for the Service Identity Key", ParameterSetName = "Certificate")]
        [Parameter(Mandatory = false, HelpMessage = "Name for the Service Identity Key", ParameterSetName = "Password")]
        [Parameter(Mandatory = false, HelpMessage = "Name for the Service Identity Key", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            if (this.ParameterSetName.Equals("ObjectParamSet", StringComparison.OrdinalIgnoreCase))
            {
                this.ManagementService.AddServiceIdentityKeyToServiceIdentity(this.ServiceIdentityKey.ToWrapperModel(), this.ServiceIdentityName);
            }
            else
            {
                switch (this.ParameterSetName.ToUpperInvariant())
                {
                    case "SYMMETRICKEY":
                        this.ManagementService.AddServiceIdentityKeyToServiceIdentity(this.Name, Convert.FromBase64String(this.Key), IdentityKeyTypes.Symmetric, IdentityKeyUsages.Signing, this.ServiceIdentityName, this.EffectiveDate, this.ExpirationDate);
                        break;

                    case "CERTIFICATE":
                        this.ManagementService.AddServiceIdentityKeyToServiceIdentity(this.Name, this.Certificate.RawData, IdentityKeyTypes.X509Certificate, IdentityKeyUsages.Signing, this.ServiceIdentityName);
                        break;

                    case "PASSWORD":
                        this.ManagementService.AddServiceIdentityKeyToServiceIdentity(this.Name, Encoding.UTF8.GetBytes(this.Password), IdentityKeyTypes.Password, IdentityKeyUsages.Password, this.ServiceIdentityName, this.EffectiveDate, this.ExpirationDate);
                        break;
                }
            }
        }
    }
}
