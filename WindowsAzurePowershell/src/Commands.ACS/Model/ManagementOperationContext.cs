using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public abstract class ManagementOperationContext
    {
        public ManagementOperationContext()
        {
        }

        public ManagementOperationContext(string mgmtToken)
        {
            this.MgmtToken = mgmtToken;
        }

        [XmlIgnore]
        public string MgmtToken { get; set; }

        public bool SystemReserved { get; set; }
    }
}
