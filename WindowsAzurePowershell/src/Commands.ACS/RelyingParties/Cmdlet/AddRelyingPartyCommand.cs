using System;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.RelyingParties.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSRelyingParty", DefaultParameterSetName = "PropertiesParamSet")]
    public class AddRelyingPartyCommand : ValueReturningCommand<RelyingParty>
    {
        [Parameter(Mandatory = true, HelpMessage = "Relying Party object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public RelyingParty RelyingParty { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name for the relying party", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The URI for which the security token that ACS issues is valid", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string Realm { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The URL to which ACS returns the security token", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string ReturnUrl { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The URL to which ACS redirects users if an error occurs during the login process", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string ErrorUrl { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Token format for ACS to use when it issues security tokens for this relying party application. SWT | SAML_1_1 | SAML_2_0", ParameterSetName = "PropertiesParamSet")]
        [ValidateSet("SWT", "SAML_1_1", "SAML_2_0")]
        public string TokenFormat { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The amount of time for a security token that ACS issues to remain valid (secs)", ParameterSetName = "PropertiesParamSet")]
        [ValidateRange(1, 1000000)]
        public int TokenLifetime { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The identity providers to use with this relying party application", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string[] AllowedIdentityProviders { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The rule group to use for this relying party application when processing claims", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string[] RuleGroupNames { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Symmetric key for token signing for this relying party application", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string SigningSymmetricKey { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "X.509 Certificate with a private key for token signing for this relying party application", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public X509Certificate2 SigningCert { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "X.509 Certificate for token encryption for this relying party application", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public X509Certificate2 EncryptionCert { get; set; }

        public override RelyingParty ExecuteProcessRecordImplementation()
        {
            if (this.RelyingParty != null)
            {
                this.ManagementService.AddRelyingParty(this.RelyingParty.ToWrapperModel());
            }
            else
            {
                this.ValidateParameters();

                var tokenFormat = (TokenType)Enum.Parse(typeof(TokenType), this.TokenFormat);

                // This is to prevent creating a relying party with default key
                var symmetricKey = string.IsNullOrEmpty(this.SigningSymmetricKey)
                    ? null
                    : Convert.FromBase64String(this.SigningSymmetricKey);


                switch (tokenFormat)
                {
                    case TokenType.SWT:
                        this.ManagementService.AddRelyingPartyWithSymmetricKey(
                            this.Name,
                            this.Realm,
                            this.ReturnUrl,
                            this.ErrorUrl,
                            tokenFormat,
                            this.TokenLifetime,
                            symmetricKey,
                            this.RuleGroupNames,
                            this.AllowedIdentityProviders);
                        break;

                    case TokenType.SAML_1_1:
                    case TokenType.SAML_2_0:
                        this.ManagementService.AddRelyingPartyWithAsymmetricKey(
                            this.Name,
                            this.Realm,
                            this.ReturnUrl,
                            this.ErrorUrl,
                            tokenFormat,
                            this.TokenLifetime,
                            this.SigningCert != null ? this.SigningCert.RawData : null,
                            string.Empty,
                            this.EncryptionCert != null ? this.EncryptionCert.RawData : null,
                            this.RuleGroupNames,
                            this.AllowedIdentityProviders);
                        break;
                }
            }

            return this.ManagementService.RetrieveRelyingParty(this.Name).ToModel(this.ManagementService.MgmtSwtToken);
        }

        private void ValidateParameters()
        {
            if (string.IsNullOrEmpty(this.TokenFormat))
            {
                this.TokenFormat = TokenType.SAML_2_0.ToString();
            }

            if (this.TokenLifetime <= 0)
            {
                this.TokenLifetime = 600;
            }
        }
    }
}
