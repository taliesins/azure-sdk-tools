using Microsoft.WindowsAzure.Storage.DataMovement;
using System;

namespace Microsoft.WindowsAzure.Storage.DataMovement.RecursiveTransferHelpers
{
	internal static class Location
	{
		private const string OnAzurePrefixHttp = "http://";

		private const string OnAzurePrefixHttps = "https://";

		public static ILocation CreateLocation(string location, string storageKey, string containerSAS, BlobTransferOptions transferOptions, bool isSourceLocation)
		{
			if (string.IsNullOrEmpty(location))
			{
				throw new ArgumentNullException("location");
			}
			if (!Location.IsOnAzure(location))
			{
				return new FileSystemLocation(location);
			}
			return new AzureStorageLocation(location, storageKey, containerSAS, transferOptions, isSourceLocation);
		}

		internal static bool Equals(ILocation locationA, ILocation locationB)
		{
			if (locationA == locationB)
			{
				return true;
			}
			if (locationA == null || locationB == null)
			{
				return false;
			}
			AzureStorageLocation azureStorageLocation = locationA as AzureStorageLocation;
			AzureStorageLocation azureStorageLocation1 = locationB as AzureStorageLocation;
			if (null == azureStorageLocation != (null == azureStorageLocation1))
			{
				return false;
			}
			if (azureStorageLocation == null)
			{
				return (locationA as FileSystemLocation).FullPath.Equals((locationB as FileSystemLocation).FullPath);
			}
			string absoluteUri = azureStorageLocation.GetAbsoluteUri(string.Empty);
			return absoluteUri.Equals(azureStorageLocation1.GetAbsoluteUri(string.Empty));
		}

		private static bool IsOnAzure(string location)
		{
			if (location.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return location.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
		}
	}
}