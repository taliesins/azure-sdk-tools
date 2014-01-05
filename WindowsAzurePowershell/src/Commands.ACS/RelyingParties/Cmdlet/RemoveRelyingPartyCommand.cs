using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.RelyingParties.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSRelyingParty")]
    public class RemoveRelyingPartyCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the relying party")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveRelyingParty(this.Name);
        }
    }
}
