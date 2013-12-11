using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceIdentitiesKeys.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSServiceIdentityKey")]
    public class GetServiceIdentityKeyCommand : ValueReturningCommand<IEnumerable<ServiceIdentityKey>>
    {
        [Parameter(ValueFromPipelineByPropertyName = true,
            HelpMessage = "Id of the Service Identity Key. Specify an Id to retrieve a single Service Identity Key")]
        [ValidateNotNullOrEmpty]
        public long? Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true,
            HelpMessage =
                "Name of the Service Identity. Specify a Service Identity to retrieve all the keys in the Service Identity"
            )]
        [ValidateNotNullOrEmpty]
        public string ServiceIdentityName { get; set; }

        public override IEnumerable<ServiceIdentityKey> ExecuteProcessRecordImplementation()
        {
            if (this.Id.HasValue)
            {
                return new[]
                {
                    this.ManagementService.RetrieveServiceIdentityKey(this.Id.Value)
                        .ToModel(this.ManagementService.MgmtSwtToken)
                };
            }
            else if (!string.IsNullOrEmpty(this.ServiceIdentityName))
            {
                return
                    this.ManagementService.RetrieveServiceIdentityKeys(this.ServiceIdentityName)
                        .Select(r => r.ToModel(this.ManagementService.MgmtSwtToken));
            }
            else
            {
                throw new PSArgumentException(
                    "Please provide either a Service Identity Key Id to retrieve a single Key, or a Service Identity Name to retrieve all the keys associated with the Service Identity");
            }
        }
    }
}