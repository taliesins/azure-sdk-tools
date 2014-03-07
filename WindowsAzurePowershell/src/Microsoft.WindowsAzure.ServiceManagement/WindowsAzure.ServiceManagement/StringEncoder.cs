using System;
using System.Text;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	internal static class StringEncoder
	{
		public static string DecodeFromBase64String(string encodedString)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
		}

		public static string EncodeToBase64String(string decodedString)
		{
			string base64String = decodedString;
			if (!string.IsNullOrEmpty(decodedString))
			{
				base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(decodedString));
			}
			return base64String;
		}

		public static bool TryDecodeFromBase64String(string encodedString, out string decodedString)
		{
			bool flag = true;
			decodedString = encodedString;
			if (!string.IsNullOrEmpty(encodedString))
			{
				try
				{
					decodedString = StringEncoder.DecodeFromBase64String(encodedString);
				}
				catch (Exception exception)
				{
					flag = false;
				}
			}
			return flag;
		}
	}
}