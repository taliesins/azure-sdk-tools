using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.IdentityProviders.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSIdentityProvider", DefaultParameterSetName = "Preconfigured")]
    public class AddIdentityProviderCommand : ValueReturningCommand<IdentityProvider>
    {
        [Parameter(Mandatory = true, HelpMessage = "Identity Provider object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public IdentityProvider IdentityProvider { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Identity Provider Type. Preconfigured | FacebookApp | WsFederation | Manual", ParameterSetName = "Preconfigured")]
        [Parameter(Mandatory = true, HelpMessage = "Identity Provider Type. Preconfigured | FacebookApp | WsFederation | Manual", ParameterSetName = "FacebookApp")]
        [Parameter(Mandatory = true, HelpMessage = "Identity Provider Type. Preconfigured | FacebookApp | WsFederation | Manual", ParameterSetName = "WsFederation")]
        [Parameter(Mandatory = true, HelpMessage = "Identity Provider Type. Preconfigured | FacebookApp | WsFederation | Manual", ParameterSetName = "Manual")]
        [ValidateSet(new string[] { "Preconfigured", "FacebookApp", "WsFederation", "Manual" })]
        public string Type { get; set; }

        [Parameter(ParameterSetName = "Preconfigured", Mandatory = true, HelpMessage = "Preconfigured identity provider. Google | Yahoo!")]
        [ValidateSet(new string[] { "Google", "Yahoo!" })]
        public string PreconfiguredIpType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Name for your identity provider", ParameterSetName = "Preconfigured")]
        [Parameter(Mandatory = false, HelpMessage = "Name for your identity provider", ParameterSetName = "FacebookApp")]
        [Parameter(Mandatory = false, HelpMessage = "Name for your identity provider", ParameterSetName = "WsFederation")]
        [Parameter(Mandatory = false, HelpMessage = "Name for your identity provider", ParameterSetName = "Manual")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Specify existing relying party applications that you want to associate with this identity provider", ParameterSetName = "Preconfigured")]
        [Parameter(Mandatory = false, HelpMessage = "Specify existing relying party applications that you want to associate with this identity provider", ParameterSetName = "FacebookApp")]
        [Parameter(Mandatory = false, HelpMessage = "Specify existing relying party applications that you want to associate with this identity provider", ParameterSetName = "WsFederation")]
        [Parameter(Mandatory = false, HelpMessage = "Specify existing relying party applications that you want to associate with this identity provider", ParameterSetName = "Manual")]
        [ValidateCount(1, 100)]
        public string[] AllowedRelyingParties { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The text to display for the login link for this identity provider", ParameterSetName = "Preconfigured")]
        [Parameter(Mandatory = false, HelpMessage = "The text to display for the login link for this identity provider", ParameterSetName = "FacebookApp")]
        [Parameter(Mandatory = false, HelpMessage = "The text to display for the login link for this identity provider", ParameterSetName = "WsFederation")]
        [Parameter(Mandatory = false, HelpMessage = "The text to display for the login link for this identity provider", ParameterSetName = "Manual")]
        public string LoginLinkText { get; set; }

        [Parameter(ParameterSetName = "FacebookApp", Mandatory = true, HelpMessage = "Facebook Application ID")]
        [ValidateNotNullOrEmpty]
        public string FbAppId { get; set; }

        [Parameter(ParameterSetName = "FacebookApp", Mandatory = true, HelpMessage = "Facebook Application Secret")]
        [ValidateNotNullOrEmpty]
        public string FbAppSecret { get; set; }

        [Parameter(ParameterSetName = "WsFederation", Mandatory = true, HelpMessage = "The WS-Federation metadata document for your server")]
        [ValidateNotNullOrEmpty]
        public string WsFederationMetadata { get; set; }

        [Parameter(ParameterSetName = "Manual", Mandatory = true, HelpMessage = "Web SSO Protocol Type. WsFederation | OAuth | OpenId | WsTrust")]
        [ValidateSet(new string[] { "WsFederation", "OAuth", "OpenId", "WsTrust" })]
        public string Protocol { get; set; }

        [Parameter(ParameterSetName = "Manual", Mandatory = true, HelpMessage = "SignIn Endpoint")]
        [ValidateNotNullOrEmpty]
        public string SignInAddress { get; set; }

        [Parameter(ParameterSetName = "Manual", Mandatory = false, HelpMessage = "X.509 Signing Certificate")]
        [ValidateNotNull]
        public X509Certificate2 SigningCertificate { get; set; }

        public override IdentityProvider ExecuteProcessRecordImplementation()
        {
            if (this.ParameterSetName.Equals("ObjectParamSet", StringComparison.OrdinalIgnoreCase))
            {
                this.ManagementService.AddIdentityProvider(this.IdentityProvider.ToWrapperModel(), this.IdentityProvider.RelyingParties.Select(r => r.Name).ToArray());
            }
            else
            {
                switch (this.Type.ToUpperInvariant())
                {
                    case "PRECONFIGURED":
                        this.AddPreconfiguredIdentityProvider();
                        break;

                    case "FACEBOOKAPP":
                        this.ValidateParameters();
                        this.ManagementService.AddFacebookIdentityProvider(this.Name, this.FbAppId, this.FbAppSecret, this.AllowedRelyingParties, this.LoginLinkText);
                        break;

                    case "WSFEDERATION":
                        this.ValidateParameters();
                        this.AddIdentityProviderFromWsFederationMetadata();
                        break;

                    case "MANUAL":
                        this.ValidateParameters();
                        this.ManagementService.AddIdentityProviderManually(
                            this.Name,
                            this.SignInAddress,
                            (WebSSOProtocolType)Enum.Parse(typeof(WebSSOProtocolType), this.Protocol),
                            this.SigningCertificate != null ? this.SigningCertificate.RawData : null,
                            this.AllowedRelyingParties,
                            this.LoginLinkText);
                        break;
                }
            }

            return this.ManagementService.RetrieveIdentityProvider(this.Name).ToModel(this.ManagementService.MgmtSwtToken);
        }

        private void ValidateParameters()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new ArgumentNullException("Name");
            }
        }

        private void AddPreconfiguredIdentityProvider()
        {
            this.Name = this.PreconfiguredIpType;
            this.ManagementService.RemoveIdentityProvider(this.Name);

            switch (this.PreconfiguredIpType.ToUpperInvariant())
            {
                case "GOOGLE":
                    this.ManagementService.AddIdentityProviderManually(
                            ConfigurationConstants.GoogleIdentityProviderName,
                            ConfigurationConstants.GoogleSignInEndpoint,
                            WebSSOProtocolType.OpenId,
                            allowedRelyingParties: this.AllowedRelyingParties,
                            loginLinkText: this.LoginLinkText);
                    break;

                case "YAHOO!":
                    this.ManagementService.AddIdentityProviderManually(
                            ConfigurationConstants.YahooIdentityProviderName,
                            ConfigurationConstants.YahooSignInEndpoint,
                            WebSSOProtocolType.OpenId,
                            allowedRelyingParties: this.AllowedRelyingParties,
                            loginLinkText: this.LoginLinkText);
                    break;

                default:
                    throw new PSInvalidOperationException("Preconfigured identity providers: Google | Yahoo!");
            }
        }

        private void AddIdentityProviderFromWsFederationMetadata()
        {
            var federationMetadataFullPath = this.ResolvePath(this.WsFederationMetadata);

            if (File.Exists(federationMetadataFullPath))
            {
                this.ManagementService.AddIdentityProvider(this.Name, File.ReadAllBytes(federationMetadataFullPath), this.AllowedRelyingParties, this.LoginLinkText);
            }
            else
            {
                throw new PSInvalidOperationException("WsFederationMetadata file does not exist. " + this.WsFederationMetadata);
            }
        }
    }
}
