using System;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class RuleGroup : ManagementOperationContext
    {
        public RuleGroup()
        {
        }

        public RuleGroup(string mgmtToken)
            : base(mgmtToken)
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public Rule[] Rules { get; set; }
    }
}
