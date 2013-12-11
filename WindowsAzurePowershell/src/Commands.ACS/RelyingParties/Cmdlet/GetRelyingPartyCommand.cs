using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.RelyingParties.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSRelyingParty")]
    public class GetRelyingPartyCommand : ValueReturningCommand<IEnumerable<RelyingParty>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the relying party. If no name is provided, all relying parties are returned")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override IEnumerable<RelyingParty> ExecuteProcessRecordImplementation()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return new[]
                {
                    this.ManagementService.RetrieveRelyingParty(this.Name).ToModel(this.ManagementService.MgmtSwtToken)
                };
            }
            else
            {
                return this.ManagementService.RetrieveRelyingParties().Select(rp => rp.ToModel(this.ManagementService.MgmtSwtToken));
            }
        }
    }
}
