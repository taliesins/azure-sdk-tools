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

namespace Microsoft.WindowsAzure.Commands.ACS
{
    public class Constants
    {
        public const string AcsHostName = "accesscontrol.windows.net";
        public const string ManagementServiceHead = "v2/mgmt/service/";
        public const string MetadataImportHeadForIp = "v2/mgmt/service/importFederationMetadata/importIdentityProvider";
        public const string MetadataImportHeadForRp = "v2/mgmt/service/importFederationMetadata/importRelyingParty";

        // seconds
        public const int RelyingPartyTokenLifetime = 600;

        public const string WindowsLiveIdIssuerName = "uri:WindowsLiveID";

        public const string IdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
    }
}