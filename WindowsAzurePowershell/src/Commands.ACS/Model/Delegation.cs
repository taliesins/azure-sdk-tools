using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class Delegation : ManagementOperationContext
    {
        public Delegation()
        {
        }

        public Delegation(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string IdentityProvider { get; set; }

        public string NameIdentifier { get; set; }

        public string Permissions { get; set; }

        public string AuthorizationCode { get; set; }

        public RelyingParty RelyingParty { get; set; }

        public ServiceIdentity ServiceIdentity { get; set; }
    }
}
