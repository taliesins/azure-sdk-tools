using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSRule")]
    public class RemoveRuleCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, HelpMessage = "Rule to remove")]
        [ValidateNotNullOrEmpty]
        public Rule Rule { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveRule(this.ManagementService.RetrieveRule(this.Rule.Id));
        }
    }
}
