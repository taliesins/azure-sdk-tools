using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.Rules.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSDefaultPassthroughRules")]
    public class AddDefaultPassthroughRulesCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, HelpMessage = "Name for the rule group")]
        [ValidateNotNullOrEmpty]
        public string GroupName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name for the identity provider")]
        [ValidateNotNullOrEmpty]
        public string IdentityProviderName { get; set; }

        public static string PassthroughRuleDescription(string identityProviderName, string inputClaimType = null)
        {
            if (inputClaimType == null)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Passthrough any claim from {0}",
                    identityProviderName);
            }
            else
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Passthrough \"{0}\" claim from {1} as \"{0}\"",
                    inputClaimType.Split('/').Last(),
                    identityProviderName);
            }
        }

        public override void ExecuteProcessRecordImplementation()
        {
            var identityProvider = this.ManagementService.RetrieveIdentityProvider(this.IdentityProviderName);
            if (identityProvider == null)
            {
                throw new PSInvalidOperationException(string.Format("The identity provider {0} doesn't exist.", this.IdentityProviderName));
            }

            var identityProviderInputClaimTypes = identityProvider.IdentityProviderClaimTypes.Select(claimType => claimType.ClaimType.Uri);
            if (identityProviderInputClaimTypes.Count() != 0)
            {
                foreach (var inputClaimType in identityProviderInputClaimTypes)
                {
                    if (!string.IsNullOrEmpty(inputClaimType))
                    {
                        var description = PassthroughRuleDescription(this.IdentityProviderName, inputClaimType);
                        this.ManagementService.AddPassthroughRuleToRuleGroup(this.GroupName, this.IdentityProviderName, inputClaimType, description: description);
                    }
                }
            }
            else
            {
                this.ManagementService.AddPassthroughRuleToRuleGroup(
                    this.GroupName,
                    this.IdentityProviderName,
                    description: PassthroughRuleDescription(this.IdentityProviderName));
            }
        }
    }
}
