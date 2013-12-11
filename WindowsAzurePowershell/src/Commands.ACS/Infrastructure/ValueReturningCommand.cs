using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.ACS.Infrastructure
{
    public abstract class ValueReturningCommand<T> : AcsCmdletBase
    {
        public abstract T ExecuteProcessRecordImplementation();

        protected override void ProcessRecord()
        {
            try
            {
                this.Initialize();
                WriteObject(this.ExecuteProcessRecordImplementation(), typeof(IEnumerable).IsAssignableFrom(typeof(T)));
            }
            catch (Exception ex)
            {
                var errorRecord = new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null);
                this.ThrowTerminatingError(errorRecord); // To throw a non termination error use this.WriteError();
            }
        }
    }
}
