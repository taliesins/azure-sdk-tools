using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	internal static class ArgumentValidator
	{
		public static void CheckIfEmptyString(string argumentName, string argumentValue)
		{
			if (argumentValue.Length <= 0)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string stringArgumentEmptyError = Resources.StringArgumentEmptyError;
				object[] objArray = new object[] { argumentName };
				throw new ArgumentException(argumentName, string.Format(invariantCulture, stringArgumentEmptyError, objArray));
			}
		}

		public static void CheckIfIntLessThanZero(string argumentName, int argumentValue)
		{
			if (argumentValue < 0)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string intArgumentLessThanZeroError = Resources.IntArgumentLessThanZeroError;
				object[] objArray = new object[] { argumentName, argumentValue };
				throw new ArgumentException(argumentName, string.Format(invariantCulture, intArgumentLessThanZeroError, objArray));
			}
		}

		public static void CheckIfNull(string argumentName, object argumentValue)
		{
			if (argumentValue == null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string argumentNullError = Resources.ArgumentNullError;
				object[] objArray = new object[] { argumentName };
				throw new ArgumentNullException(argumentName, string.Format(invariantCulture, argumentNullError, objArray));
			}
		}
	}
}