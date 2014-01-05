
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.ACS
{
    public class AcsCmdletBase : PSCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Management SWT Token")]
        [ValidateNotNullOrEmpty]
        public string MgmtToken { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of your service namespace (e.g.: <namespace>.accesscontrol.windows.net)")]
        [ValidateNotNullOrEmpty]
        public string Namespace { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Symmetric Key to authenticate with Access Control Service management")]
        [ValidateNotNullOrEmpty]
        public string ManagementKey { get; set; }

        protected ServiceManagementWrapper ManagementService { get; private set; }

        protected void Initialize()
        {
            if (string.IsNullOrEmpty(this.MgmtToken))
            {
                if (string.IsNullOrEmpty(this.Namespace))
                {
                    throw new PSArgumentNullException("Namespace");
                }

                if (string.IsNullOrEmpty(this.ManagementKey))
                {
                    throw new PSArgumentNullException("ManagementKey");
                }        

                var managementClientName = string.Empty;
                if (Namespace.EndsWith("-sb", true, CultureInfo.CurrentCulture))
                {
                    managementClientName = "SB";
                }

                managementClientName = managementClientName + "ManagementClient";

                this.ManagementService = new ServiceManagementWrapper(this.Namespace, managementClientName, this.ManagementKey);
           
            }
            else
            {
                this.ManagementService = new ServiceManagementWrapper(this.MgmtToken);
            }
        }
    }
}
