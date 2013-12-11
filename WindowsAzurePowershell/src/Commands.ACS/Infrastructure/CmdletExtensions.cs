using System.Globalization;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.ACS.Infrastructure
{
    public static class CmdletExtensions
    {
        public static string ResolvePath(this PSCmdlet cmdlet, string path)
        {
            var result = cmdlet.SessionState.Path.GetResolvedPSPathFromPSPath(path);
            string fullPath = string.Empty;

            if (result != null && result.Count > 0)
            {
                fullPath = result[0].Path;
            }

            return fullPath;
        }

        public static void WriteVerbose(this PSCmdlet cmdlet, string format, params object[] args)
        {
            var text = string.Format(CultureInfo.InvariantCulture, format, args);
            cmdlet.WriteVerbose(text);
        }
    }
}
