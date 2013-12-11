using System;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ACS.Infrastructure;
using Microsoft.WindowsAzure.Commands.ACS.Model;

namespace Microsoft.WindowsAzure.Commands.ACS.ServiceIdentities.Cmdlet
{
    [Cmdlet(VerbsCommon.Add, "AzureACSServiceIdentity", DefaultParameterSetName = "PropertiesParamSet")]
    public class AddServiceIdentityCommand : ValueReturningCommand<ServiceIdentity>
    {
        [Parameter(Mandatory = true, HelpMessage = "Service Identity object to add", ParameterSetName = "ObjectParamSet")]
        [ValidateNotNullOrEmpty]
        public ServiceIdentity ServiceIdentity { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name for the service identity", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Description for the service identity", ParameterSetName = "PropertiesParamSet")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        public override ServiceIdentity ExecuteProcessRecordImplementation()
        {
            if (this.ParameterSetName.Equals("ObjectParamSet", StringComparison.OrdinalIgnoreCase))
            {
                this.ManagementService.AddServiceIdentity(this.ServiceIdentity.ToWrapperModel());
            }
            else
            {
                this.ManagementService.AddServiceIdentity(this.Name, this.Description);
            }

            return this.ManagementService.RetrieveServiceIdentity(this.Name).ToModel(this.ManagementService.MgmtSwtToken);
        }
    }
}
