using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;

namespace Microsoft.WindowsAzure.Commands.ACS.Management.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "AzureACSManagementToken")]
    public class GetAcsManagementTokenCommand : ValueReturningCommand<string>
    {
        public override string ExecuteProcessRecordImplementation()
        {
            return this.ManagementService.MgmtSwtToken;
        }
    }
}
