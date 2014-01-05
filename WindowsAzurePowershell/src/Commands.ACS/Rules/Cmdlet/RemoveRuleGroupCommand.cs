using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSRuleGroup")]
    public class RemoveRuleGroupCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the rule group")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveExistantRuleGroup(this.Name);
        }
    }
}
