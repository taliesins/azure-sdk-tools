using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.IdentityProviders.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSIdentityProvider")]
    public class RemoveIdentityProviderCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the identity provider")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveIdentityProvider(this.Name);
        }
    }
}
