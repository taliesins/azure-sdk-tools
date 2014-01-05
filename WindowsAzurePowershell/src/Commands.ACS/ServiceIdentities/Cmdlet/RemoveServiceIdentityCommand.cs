using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceIdentities.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSServiceIdentity")]
    public class RemoveServiceIdentityCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the service identity")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveServiceIdentity(this.Name);
        }
    }
}
