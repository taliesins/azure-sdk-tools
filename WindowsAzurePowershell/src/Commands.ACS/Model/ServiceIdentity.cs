using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class ServiceIdentity : ManagementOperationContext
    {
        public ServiceIdentity()
        {
        }

        public ServiceIdentity(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string RedirectAddress { get; set; }

        public ServiceIdentityKey[] ServiceIdentityKeys { get; set; }

        public Delegation[] Delegations { get; set; }
    }
}
