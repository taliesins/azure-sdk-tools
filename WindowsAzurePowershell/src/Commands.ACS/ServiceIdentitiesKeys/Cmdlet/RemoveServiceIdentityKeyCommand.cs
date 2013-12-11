using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceIdentitiesKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSServiceIdentityKey")]
    public class RemoveServiceIdentityKeyCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, HelpMessage = "Id of the Service Identity Key to remove.")]
        [ValidateNotNullOrEmpty]
        public long Id { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveServiceIdentityKey(this.Id);
        }
    }
}
