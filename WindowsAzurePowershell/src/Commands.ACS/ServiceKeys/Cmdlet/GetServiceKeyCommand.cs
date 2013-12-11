using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSServiceKey")]
    public class GetServiceKeyCommand : ValueReturningCommand<IEnumerable<ServiceKey>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the service key")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override IEnumerable<ServiceKey> ExecuteProcessRecordImplementation()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return new[]
                {
                    this.ManagementService.RetrieveServiceKey(this.Name).ToModel(this.ManagementService.MgmtSwtToken)
                };
            }
            else
            {
                return this.ManagementService.RetrieveServiceKeys().Select(sk => sk.ToModel(this.ManagementService.MgmtSwtToken));
            }
        }
    }
}
