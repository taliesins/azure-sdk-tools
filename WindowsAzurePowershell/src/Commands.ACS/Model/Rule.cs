using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class Rule : ManagementOperationContext
    {
        public Rule()
        {
        }

        public Rule(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Description { get; set; }

        public string IdentityProviderName { get; set; }

        public string InputClaimType { get; set; }

        public string InputClaimValue { get; set; }

        public string OutputClaimType { get; set; }

        public string OutputClaimValue { get; set; }
    }
}
