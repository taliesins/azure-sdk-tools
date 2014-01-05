using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class RelyingParty : ManagementOperationContext
    {
        public RelyingParty()
        {
        }

        public RelyingParty(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public bool AsymmetricTokenEncryptionRequired { get; set; }

        public string Realm { get; set; }

        public string ReturnUrl { get; set; }

        public string ErrorUrl { get; set; }

        public string TokenType { get; set; }

        public int TokenLifetime { get; set; }

        public IdentityProvider[] IdentityProviders { get; set; }

        public ServiceKey[] Keys { get; set; }

        public RuleGroup[] RuleGroups { get; set; }
    }
}
