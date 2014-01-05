using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class IdentityProvider : ManagementOperationContext
    {
        public IdentityProvider()
        {
        }

        public IdentityProvider(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Realm { get; set; }

        public string WebSSOProtocolType { get; set; }

        public string LoginLinkName { get; set; }

        public string LoginParameters { get; set; }

        public string IssuerName { get; set; }

        public string SignInEndpoint { get; set; }

        public string SignOutEndpoint { get; set; }

        public string EmailDomain { get; set; }

        public string ImageUrl { get; set; }

        public string FedMetadataUrl { get; set; }

        public string[] ClaimTypes { get; set; }

        public ServiceKey[] Keys { get; set; }

        public RelyingParty[] RelyingParties { get; set; }
    }
}
