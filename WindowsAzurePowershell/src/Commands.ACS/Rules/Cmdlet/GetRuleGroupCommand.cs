using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSRuleGroup")]
    public class GetRuleGroupCommand : ValueReturningCommand<IEnumerable<RuleGroup>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the rule group. If no name is provided, all rule groups are returned")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override IEnumerable<RuleGroup> ExecuteProcessRecordImplementation()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return new[]
                {
                    this.ManagementService.RetrieveRuleGroup(this.Name).ToModel(this.ManagementService.MgmtSwtToken)
                };
            }
            else
            {
                return this.ManagementService.RetrieveRuleGroups().Select(rg => rg.ToModel(this.ManagementService.MgmtSwtToken));
            }
        }
    }
}
