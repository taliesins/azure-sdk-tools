using System;
using System.Globalization;
using System.IO;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	internal static class Utils
	{
		private static string[] sizeFormats;

		static Utils()
		{
			string[] readableSizeFormatBytes = new string[] { Resources.ReadableSizeFormatBytes, Resources.ReadableSizeFormatKiloBytes, Resources.ReadableSizeFormatMegaBytes, Resources.ReadableSizeFormatGigaBytes, Resources.ReadableSizeFormatTeraBytes, Resources.ReadableSizeFormatPetaBytes, Resources.ReadableSizeFormatExaBytes };
			Utils.sizeFormats = readableSizeFormatBytes;
		}

		public static string AppendSnapShotToFileName(string fileName, DateTimeOffset? snapshotTime)
		{
			string str = fileName;
			if (snapshotTime.HasValue)
			{
				string str1 = Path.ChangeExtension(fileName, null);
				string extension = Path.GetExtension(fileName);
				string str2 = string.Format("{0:yyyy-MM-dd HHmmss fff}", snapshotTime.Value);
				str = string.Format("{0} ({1}){2}", str1, str2, extension);
			}
			return str;
		}

		public static string BytesToHumanReadableSize(double size)
		{
			int num = 0;
			while (size >= 1024 && num + 1 < (int)Utils.sizeFormats.Length)
			{
				num++;
				size = size / 1024;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			string str = Utils.sizeFormats[num];
			object[] objArray = new object[] { size };
			return string.Format(invariantCulture, str, objArray);
		}
	}
}