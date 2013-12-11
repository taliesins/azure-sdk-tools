using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class ServiceKey : ManagementOperationContext
    {
        public ServiceKey()
        {
        }

        public ServiceKey(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Usage { get; set; }

        public byte[] Value { get; set; }

        public string Password { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
