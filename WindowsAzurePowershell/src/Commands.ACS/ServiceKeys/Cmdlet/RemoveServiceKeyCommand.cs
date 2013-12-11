using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "AzureACSServiceKey")]
    public class RemoveServiceKeyCommand : CommandNotReturningValue
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the service key")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteProcessRecordImplementation()
        {
            this.ManagementService.RemoveServiceKey(this.Name);
        }
    }
}
