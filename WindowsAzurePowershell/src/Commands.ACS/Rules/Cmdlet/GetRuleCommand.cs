using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSRule")]
    public class GetRuleCommand : ValueReturningCommand<IEnumerable<Rule>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Id of the rule. Specify an Id to retrieve a single rule")]
        [ValidateNotNullOrEmpty]
        public long? Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the rule group. Specify a rule group to retrieve all the rules in the group")]
        [ValidateNotNullOrEmpty]
        public string GroupName { get; set; }

        public override IEnumerable<Rule> ExecuteProcessRecordImplementation()
        {
            if (this.Id.HasValue)
            {
                return new[]
                {
                    this.ManagementService.RetrieveRule(this.Id.Value).ToModel(this.ManagementService.MgmtSwtToken)
                };
            }
            else if (!string.IsNullOrEmpty(this.GroupName))
            {
                return this.ManagementService.RetrieveRules(this.GroupName).Select(r => r.ToModel(this.ManagementService.MgmtSwtToken));
            }
            else
            {
                throw new PSArgumentException("Please provide either a rule Id to retrieve a single rule, or a rule group to retrieve all the rules in the group");
            }
        }
    }
}
