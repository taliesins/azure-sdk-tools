using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceIdentities.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSServiceIdentity")]
    public class GetServiceIdentityCommand : ValueReturningCommand<IEnumerable<ServiceIdentity>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the service identity. If no name is provided, all service identities are returned")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override IEnumerable<ServiceIdentity> ExecuteProcessRecordImplementation()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return new[]
                    {
                        this.ManagementService.RetrieveServiceIdentity(this.Name).ToModel(this.ManagementService.MgmtSwtToken) 
                    };
            }
            else
            {
                return this.ManagementService.RetrieveServiceIdentities().Select(si => si.ToModel(this.ManagementService.MgmtSwtToken));
            }
        }
    }
}
