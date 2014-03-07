using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal interface ILocation
	{
		IEnumerable<FileEntry> EnumerateLocation(IEnumerable<string> filePatterns, bool recursive, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource);

		IEnumerable<string> GetFilePatternWithDefault(IEnumerable<string> filePatterns);

		int GetMaxFileNameLength();
	}
}