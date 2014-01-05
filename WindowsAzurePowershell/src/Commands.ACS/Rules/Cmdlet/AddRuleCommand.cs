using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSRule", DefaultParameterSetName = "ObjectParamSet")]
    public class AddRuleCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, HelpMessage = "Rule object to be added", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public Rule Rule { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name of the rule group where the rule will be added")]
        [ValidateNotNullOrEmpty]
        public string GroupName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name of the identity provider", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string IdentityProviderName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Description of the rule", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string Description { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Input claim type", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string InputClaimType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Input claim value", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string InputClaimValue { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Output claim type", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string OutputClaimType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Input claim value", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNull]
        public string OutputClaimValue { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            if (this.Rule != null)
            {
                this.ManagementService.AddRuleToRuleGroup(this.Rule.ToWrapperModel(), this.GroupName);
            }
            else
            {
                this.ManagementService.AddSimpleRuleToRuleGroup(
                    this.GroupName,
                    this.IdentityProviderName,
                    this.InputClaimType,
                    this.InputClaimValue,
                    this.OutputClaimType,
                    this.OutputClaimValue,
                    this.Description);
            }
        }
    }
}
