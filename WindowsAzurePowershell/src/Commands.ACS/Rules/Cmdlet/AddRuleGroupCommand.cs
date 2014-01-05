using System;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSRuleGroup", DefaultParameterSetName = "PropertiesParamSet")]
    public class AddRuleGroupCommand : ValueReturningCommand<RuleGroup>
    {
        [Parameter(Mandatory = true, HelpMessage = "RuleGroup object to be added", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public RuleGroup RuleGroup { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name for the rule group", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override RuleGroup ExecuteProcessRecordImplementation()
        {
            if (this.ParameterSetName.Equals("ObjectParamSet", StringComparison.OrdinalIgnoreCase))
            {
                this.ManagementService.AddRuleGroup(this.RuleGroup.ToWrapperModel());
            }
            else
            {
                this.ManagementService.AddRuleGroup(this.Name);
            }

            return this.ManagementService.RetrieveRuleGroup(this.Name).ToModel(this.ManagementService.MgmtSwtToken);
        }
    }
}
