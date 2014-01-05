using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.ACS.Infrastructure
{
    public abstract class CommandNotReturningValue : AcsCmdletBase
    {
        public abstract void ExecuteProcessRecordImplementation();

        protected override void ProcessRecord()
        {
            try
            {
                this.Initialize();
                this.ExecuteProcessRecordImplementation();
            }
            catch (Exception ex)
            {
                var errorRecord = new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null);
                this.ThrowTerminatingError(errorRecord); // To throw a non termination error use this.WriteError();
            }
        }
    }
}
