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
    public enum KeyType
    {
        // Recommend not to share symmetric signing key across RPs but configure it on RP instead. 
        Symmetric,

        X509Certificate,

        ApplicationKey
    }

    public enum KeyUsage
    {
        // Used for signing tokens issued to RPs. 
        Signing,

        // Used for decrypting tokens issued by IDPs. 
        Encrypting,

        ApplicationId,

        ApplicationSecret
    }

    public enum RuleTypes
    {
        Simple,
        Passthrough
    }

    public enum RelyingPartyAddressEndpointType
    {
        Realm,
        Reply,
        Error
    }

    public enum WebSSOProtocolType
    {
        WsFederation,
        OAuth,
        OpenId,
        Facebook,
        WsTrust
    }

    public enum TokenType
    {
        SWT,
        SAML_1_1,
        SAML_2_0
    }

    public enum EndpointType
    {
        SignIn,
        SignOut,
        EmailDomain,
        ImageUrl,
        FedMetadataUrl
    }

    public enum IdentityKeyTypes
    {
        // X509Certificate service identity is not supported by ACSv2 yet, although database scheme allows it.
        X509Certificate,

        Password,

        // Used for supporting Wrap Profile 5.2 - SWT assertion. 
        // Instead of sending username and password, clients can send a signed SWT assertion in a request. 
        Symmetric
    }

    public enum IdentityKeyUsages
    {
        Signing,
        Password
    }
}