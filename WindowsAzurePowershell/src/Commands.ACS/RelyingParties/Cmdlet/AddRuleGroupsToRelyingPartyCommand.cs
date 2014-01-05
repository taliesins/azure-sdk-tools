using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.RelyingParties.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSRuleGroupsToRelyingParty", DefaultParameterSetName = "PropertiesParamSet")]
    public class AddRuleGroupsToRelyingParty : ValueReturningCommand<RelyingParty>
    {
        [Parameter(Mandatory = true, HelpMessage = "Relying Party object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public string RelyingPartyName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The rule group to use for this relying party application when processing claims", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public string[] RuleGroupNames { get; set; }

        public override RelyingParty ExecuteProcessRecordImplementation()
        {
            var relyingParty = this.ManagementService.AddRuleGroupsToRelyingParty(this.RuleGroupNames, this.RelyingPartyName);
            return relyingParty.ToModel(this.MgmtToken);
        }
    }
}
