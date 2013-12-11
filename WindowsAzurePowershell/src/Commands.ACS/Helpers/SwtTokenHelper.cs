// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.ACS.Helpers
{
    public static class SwtTokenHelper
    {
        public static string GetTokenProperties(string swtToken, string propertyName)
        {
            var swt = swtToken.Substring("wrap_access_token=\"".Length, swtToken.Length - ("wrap_access_token=\"".Length + 1));
            var tokenValue = Uri.UnescapeDataString(swt);
            var properties = (from prop in tokenValue.Split('&')
                              let pair = prop.Split(new[] { '=' }, 2)
                              select new { Name = pair[0].ToLowerInvariant(), Value = pair[1] })
                             .ToDictionary(p => p.Name, p => p.Value);

            return properties.ContainsKey(propertyName.ToLowerInvariant()) ?
                properties[propertyName.ToLowerInvariant()] :
                string.Empty;
        }

        public static DateTime GetExpiryTime(string swtToken)
        {
            var expiresOn = GetTokenProperties(swtToken, "ExpiresOn");

            // According to the OAuth WRAP Spec, this is a Unix time (i.e.: the number of seconds past January 1, 1970 at 12am)
            var expiresOnUnixTicks = int.Parse(expiresOn, CultureInfo.InvariantCulture);
            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);

            return epochStart.AddSeconds(expiresOnUnixTicks);
        }
    }
}