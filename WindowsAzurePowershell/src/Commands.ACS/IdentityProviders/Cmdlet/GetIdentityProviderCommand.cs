using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.IdentityProviders.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSIdentityProvider")]
    public class GetIdentityProviderCommand : ValueReturningCommand<IEnumerable<IdentityProvider>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name for the identity provider. If no name is provided, all indentity providers are returned")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override IEnumerable<IdentityProvider> ExecuteProcessRecordImplementation()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return new[] 
                {
                    this.ManagementService.RetrieveIdentityProvider(this.Name).ToModel(this.ManagementService.MgmtSwtToken) 
                };
            }
            else
            {
                return this.ManagementService.RetrieveIdentityProviders().Select(ip => ip.ToModel(this.ManagementService.MgmtSwtToken));
            }
        }
    }
}
