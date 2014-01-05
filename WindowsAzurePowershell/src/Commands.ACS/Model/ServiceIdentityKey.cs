using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class ServiceIdentityKey : ManagementOperationContext
    {
        public ServiceIdentityKey()
        {
        }

        public ServiceIdentityKey(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Type { get; set; }

        public string Usage { get; set; }

        public byte[] Value { get; set; }
    }
}
