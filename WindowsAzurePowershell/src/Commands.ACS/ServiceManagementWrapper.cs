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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using Microsoft.WindowsAzure.Commands.ACS.Helpers;
using Microsoft.WindowsAzure.Commands.ACS.Service;

namespace Microsoft.WindowsAzure.Commands.ACS
{
    public class ServiceManagementWrapper
    {
        private readonly string _serviceNamespace;
        private readonly string _serviceIdentityUsernameForManagement;
        private readonly string _serviceIdentityPasswordForManagement;

        public ServiceManagementWrapper(string serviceNamespace, string serviceIdentityUsernameForManagement, string serviceIdentityPasswordForManagement)
        {
            if (string.IsNullOrEmpty(serviceNamespace))
            {
                throw new ArgumentNullException("serviceNamespace");
            }

            if (string.IsNullOrEmpty(serviceIdentityUsernameForManagement))
            {
                throw new ArgumentNullException("serviceIdentityUsernameForManagement");
            }

            if (string.IsNullOrEmpty(serviceIdentityPasswordForManagement))
            {
                throw new ArgumentNullException("serviceIdentityPasswordForManagement");
            }
            
            _serviceNamespace = serviceNamespace;
            _serviceIdentityUsernameForManagement = serviceIdentityUsernameForManagement;
            _serviceIdentityPasswordForManagement = serviceIdentityPasswordForManagement;
            
            MgmtSwtToken = GetTokenFromACS();
        }

        public ServiceManagementWrapper(string mgmtToken)
        {
            if (string.IsNullOrEmpty(mgmtToken))
            {
                throw new ArgumentNullException("mgmtToken");
            }

            _serviceNamespace = GetAcsNamespace(mgmtToken);
            MgmtSwtToken = mgmtToken;
        }

        public string MgmtSwtToken { get; private set; }

        public Issuer AddIdentityProvider(string displayName, byte[] fedMetadata, string[] allowedRelyingParties = null, string loginLinkText = "")
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                string metadataImporter = string.Format(
                    "https://{0}.{1}/{2}",
                    this._serviceNamespace,
                    Constants.AcsHostName,
                    Constants.MetadataImportHeadForIp);

                var issuerName = this.ImportWsFederationMetadata(fedMetadata, metadataImporter);
                var newIdentityProvider = client.IdentityProviders.Expand("Issuer").Where(idp => idp.Issuer.Name.Equals(issuerName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (newIdentityProvider == null)
                {
                    throw new InvalidOperationException("Identity Provider with issuer: " + issuerName + " does not exist");
                }

                // Update properties
                newIdentityProvider.DisplayName = displayName;
                newIdentityProvider.LoginLinkName = string.IsNullOrEmpty(loginLinkText) ? displayName : loginLinkText;
                client.UpdateObject(newIdentityProvider);
                client.SaveChanges();

                // if no allowed relying parties were set, allow all
                if (allowedRelyingParties == null)
                {
                    allowedRelyingParties = client.RelyingParties.ToArray().Select(idp => idp.Name).ToArray();
                }

                foreach (var allowedRp in allowedRelyingParties)
                {
                    var rp = client.RelyingParties
                                    .Where(i => i.Name == allowedRp)
                                    .FirstOrDefault();

                    if (rp != null)
                    {
                        var rpidp = new RelyingPartyIdentityProvider
                        {
                            IdentityProviderId = newIdentityProvider.Id,
                            RelyingPartyId = rp.Id
                        };

                        client.AddToRelyingPartyIdentityProviders(rpidp);
                    }
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                return newIdentityProvider.Issuer;
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public RelyingParty AddRuleGroupsToRelyingParty(string[] ruleGroupNames, string relyingPartyName)
        {
            var client = CreateManagementServiceClient();
            var relyingParty = this.RetrieveRelyingParty(relyingPartyName);

            foreach (var ruleGroupName in ruleGroupNames)
            {
                var ruleGroup = this.RetrieveRuleGroup(ruleGroupName);

                var relyingPartyRuleGroup = new RelyingPartyRuleGroup()
                {
                    RelyingPartyId = relyingParty.Id,
                    RuleGroupId = ruleGroup.Id
                };

                client.AddToRelyingPartyRuleGroups(relyingPartyRuleGroup);
                
            }
            client.SaveChanges();

            return relyingParty;
        }

        public Issuer AddFacebookIdentityProvider(string displayName, string facebookAppId, string facebookAppSecret, string[] allowedRelyingParties = null, string loginLinkText = "")
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                var issuer = new Issuer
                {
                    Name = displayName
                };

                var oldIssuer = client.Issuers.Where(ip => ip.Name == issuer.Name).FirstOrDefault();
                if (oldIssuer != null)
                {
                    client.DeleteObject(oldIssuer);
                }

                client.AddToIssuers(issuer);
                client.SaveChanges(SaveChangesOptions.Batch);

                var facebook = new IdentityProvider
                {
                    DisplayName = displayName,
                    LoginLinkName = string.IsNullOrEmpty(loginLinkText) ? displayName : loginLinkText,
                    LoginParameters = "email",
                    WebSSOProtocolType = WebSSOProtocolType.Facebook.ToString(),
                    IssuerId = issuer.Id
                };

                var oldIdentityProvider = client.IdentityProviders.Where(ip => ip.DisplayName.Equals(facebook.DisplayName, StringComparison.OrdinalIgnoreCase))
                                                                  .FirstOrDefault();
                if (oldIdentityProvider != null)
                {
                    client.DeleteObject(oldIdentityProvider);
                    client.SaveChanges();
                }

                client.AddObject("IdentityProviders", facebook);
                client.SaveChanges();

                var facebookAddress = new IdentityProviderAddress
                {
                    Address = "https://graph.facebook.com/oauth/authorize",
                    EndpointType = EndpointType.SignIn.ToString(),
                    IdentityProviderId = facebook.Id
                };

                client.AddToIdentityProviderAddresses(facebookAddress);
                client.SaveChanges();

                var facebookKeys = new[]
                {
                    new IdentityProviderKey
                        {
                            IdentityProviderId = facebook.Id,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddYears(1),
                            Type = KeyType.ApplicationKey.ToString(),
                            Usage = KeyUsage.ApplicationId.ToString(),
                            Value = Encoding.Default.GetBytes(facebookAppId)
                        },
                    new IdentityProviderKey
                        {
                            IdentityProviderId = facebook.Id,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddYears(1),
                            Type = KeyType.ApplicationKey.ToString(),
                            Usage = KeyUsage.ApplicationSecret.ToString(),
                            Value = Encoding.Default.GetBytes(facebookAppSecret)
                        }
                };

                foreach (var key in facebookKeys)
                {
                    client.AddToIdentityProviderKeys(key);
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                // if no allowed relying parties were set, allow all
                if (allowedRelyingParties == null)
                {
                    allowedRelyingParties = client.RelyingParties.ToArray().Select(idp => idp.Name).ToArray();
                }

                foreach (var allowedRp in allowedRelyingParties)
                {
                    var rp = client.RelyingParties
                                    .Where(i => i.Name == allowedRp)
                                    .FirstOrDefault();

                    if (rp != null)
                    {
                        var rpidp = new RelyingPartyIdentityProvider
                        {
                            IdentityProviderId = facebook.Id,
                            RelyingPartyId = rp.Id
                        };

                        client.AddToRelyingPartyIdentityProviders(rpidp);
                    }
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                return issuer;
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public Issuer AddIdentityProviderManually(string displayName, string federationUrl, WebSSOProtocolType protocolType, byte[] signingValidationCert = null, string[] allowedRelyingParties = null, string loginLinkText = "")
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var defaultStartDate = DateTime.UtcNow;
                var defaultEndDate = defaultStartDate.AddYears(1);

                var issuer = new Issuer
                {
                    Name = displayName
                };

                var oldIssuer = client.Issuers.Where(ip => ip.Name == issuer.Name).FirstOrDefault();
                if (oldIssuer != null)
                {
                    client.DeleteObject(oldIssuer);
                }

                client.AddToIssuers(issuer);
                client.SaveChanges(SaveChangesOptions.Batch);

                var identityProvider = new IdentityProvider
                {
                    DisplayName = displayName,
                    WebSSOProtocolType = protocolType.ToString(),
                    LoginLinkName = string.IsNullOrEmpty(loginLinkText) ? displayName : loginLinkText,
                    IssuerId = issuer.Id
                };

                var oldIdentityProvider = client.IdentityProviders.Where(ip => ip.DisplayName.Equals(identityProvider.DisplayName, StringComparison.OrdinalIgnoreCase))
                                                                  .FirstOrDefault();
                if (oldIdentityProvider != null)
                {
                    client.DeleteObject(oldIdentityProvider);
                    client.SaveChanges();
                }

                client.AddToIdentityProviders(identityProvider);
                client.SaveChanges();

                // Identity provider public key to verify the signature
                if (signingValidationCert != null)
                {
                    var key = new IdentityProviderKey
                    {
                        IdentityProviderId = identityProvider.Id,
                        DisplayName = "Signing Key for " + displayName,
                        StartDate = defaultStartDate,
                        EndDate = defaultEndDate,
                        Type = KeyType.X509Certificate.ToString(),
                        Usage = KeyUsage.Signing.ToString(),
                        Value = signingValidationCert
                    };

                    client.AddToIdentityProviderKeys(key);
                }

                // WS-Federation sign-in URL
                var federationSignInAddress = new IdentityProviderAddress
                {
                    IdentityProviderId = identityProvider.Id,
                    EndpointType = EndpointType.SignIn.ToString(),
                    Address = federationUrl
                };

                client.AddToIdentityProviderAddresses(federationSignInAddress);

                client.SaveChanges(SaveChangesOptions.Batch);

                // if no allowed relying parties were set, allow all
                if (allowedRelyingParties == null)
                {
                    allowedRelyingParties = client.RelyingParties.ToArray().Select(idp => idp.Name).ToArray();
                }

                foreach (var allowedRp in allowedRelyingParties)
                {
                    var rp = client.RelyingParties
                                    .Where(i => i.Name == allowedRp)
                                    .FirstOrDefault();

                    if (rp != null)
                    {
                        var rpidp = new RelyingPartyIdentityProvider
                        {
                            IdentityProviderId = identityProvider.Id,
                            RelyingPartyId = rp.Id
                        };

                        client.AddToRelyingPartyIdentityProviders(rpidp);
                    }
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                return issuer;
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveIdentityProvider(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                this.RemoveIssuer(name);

                IdentityProvider identityProvider = client.IdentityProviders
                                                          .Where(m => m.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                                                          .FirstOrDefault();

                if (identityProvider != null)
                {
                    client.DeleteObject(identityProvider);
                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveIssuer(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                Issuer issuer = client.Issuers
                                        .Where(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                                        .FirstOrDefault();

                if (issuer == null)
                {
                    var identityProvider = client.IdentityProviders
                                                .Expand("Issuer")
                                                .Where(ip => ip.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                                                .FirstOrDefault();

                    if (identityProvider != null)
                    {
                        issuer = identityProvider.Issuer;
                    }
                }

                if (issuer != null)
                {
                    client.DeleteObject(issuer);
                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<IdentityProvider> RetrieveIdentityProviders()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.IdentityProviders.Expand("IdentityProviderAddresses,IdentityProviderClaimTypes/ClaimType,IdentityProviderKeys,Issuer");

                return RetrieveAllResults<IdentityProvider>(client, query as DataServiceQuery<IdentityProvider>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<IdentityProvider> RetrieveBusinessIdentityProviders()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.IdentityProviders
                    .Expand("IdentityProviderAddresses,IdentityProviderClaimTypes/ClaimType,IdentityProviderKeys,Issuer,RelyingPartyIdentityProviders/RelyingParty/RelyingPartyAddresses")
                    .Where(m =>
                        (m.WebSSOProtocolType.Equals(WebSSOProtocolType.WsFederation.ToString(), StringComparison.OrdinalIgnoreCase) && (!m.Issuer.Name.Equals(Constants.WindowsLiveIdIssuerName, StringComparison.OrdinalIgnoreCase))) ||
                        m.WebSSOProtocolType.Equals(WebSSOProtocolType.WsTrust.ToString(), StringComparison.OrdinalIgnoreCase));

                return RetrieveAllResults<IdentityProvider>(client, query as DataServiceQuery<IdentityProvider>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IdentityProvider RetrieveIdentityProvider(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.IdentityProviders
                    .Expand("IdentityProviderAddresses,IdentityProviderClaimTypes/ClaimType,IdentityProviderKeys,Issuer")
                    .Where(m => m.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase) || (m.Issuer != null && m.Issuer.Name.Equals(name, StringComparison.Ordinal)))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddRelyingPartyWithSymmetricKey(string relyingPartyName, string realmAddress, byte[] symmetricKey, string[] ruleGroupNames = null, string[] allowedIdentityProviders = null)
        {
            this.AddRelyingPartyWithSymmetricKey(relyingPartyName, realmAddress, string.Empty, string.Empty, TokenType.SWT, Constants.RelyingPartyTokenLifetime, symmetricKey, ruleGroupNames, allowedIdentityProviders);
        }

        public void AddRelyingPartyWithSymmetricKey(string relyingPartyName, string realmAddress, string replyAddress, string errorAddress, TokenType tokenType, int tokenLifetime, byte[] symmetricKey, string[] ruleGroupNames = null, string[] allowedIdentityProviders = null)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                var defaultStartDate = DateTime.UtcNow;
                var defaultEndDate = defaultStartDate.AddYears(1);

                RelyingParty relyingParty;
                CreateRelyingParty(client, relyingPartyName, ruleGroupNames, realmAddress, replyAddress, tokenType, errorAddress, tokenLifetime, false, out relyingParty);

                if (symmetricKey != null)
                {
                    AddSigningKeyToRelyingParty(client, symmetricKey, defaultStartDate, defaultEndDate, relyingParty);
                }

                client.SaveChanges();

                AddIdentityProviderToRelyingParty(client, allowedIdentityProviders, relyingParty);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddRelyingParty(RelyingParty relyingParty)
        {
            try
            {
                // Validate that IP and Rule Groups exist
                foreach (var relyingPartyIdProvider in relyingParty.RelyingPartyIdentityProviders)
                {
                    var identityProvider = this.RetrieveIdentityProvider(relyingPartyIdProvider.IdentityProvider.DisplayName);
                    if (identityProvider == null)
                    {
                        throw new InvalidOperationException("Identity Provider: " + relyingPartyIdProvider.IdentityProvider.DisplayName + " does not exist");
                    }

                    relyingPartyIdProvider.IdentityProvider = identityProvider;
                    relyingPartyIdProvider.IdentityProviderId = identityProvider.Id;
                }

                foreach (var relyingPartyRuleGroup in relyingParty.RelyingPartyRuleGroups)
                {
                    var ruleGroup = this.RetrieveRuleGroup(relyingPartyRuleGroup.RuleGroup.Name);
                    if (ruleGroup == null)
                    {
                        throw new InvalidOperationException("Rule Group: " + relyingPartyRuleGroup.RuleGroup.Name + " does not exist");
                    }

                    relyingPartyRuleGroup.RuleGroup = ruleGroup;
                    relyingPartyRuleGroup.RuleGroupId = ruleGroup.Id;
                }

                var client = this.CreateManagementServiceClient();
                client.AddToRelyingParties(relyingParty);
                client.SaveChanges();

                foreach (var relyingPartyKey in relyingParty.RelyingPartyKeys)
                {
                    relyingPartyKey.RelyingPartyId = relyingParty.Id;
                    client.AddToRelyingPartyKeys(relyingPartyKey);
                }

                client.SaveChanges();

                foreach (var relyingPartyIdProvider in relyingParty.RelyingPartyIdentityProviders)
                {
                    relyingPartyIdProvider.RelyingParty = relyingParty;
                    relyingPartyIdProvider.RelyingPartyId = relyingParty.Id;
                    client.AddToRelyingPartyIdentityProviders(relyingPartyIdProvider);
                }

                client.SaveChanges();

                foreach (var relyingPartyRuleGroup in relyingParty.RelyingPartyRuleGroups)
                {
                    relyingPartyRuleGroup.RelyingParty = relyingParty;
                    relyingPartyRuleGroup.RelyingPartyId = relyingParty.Id;
                    client.AddToRelyingPartyRuleGroups(relyingPartyRuleGroup);
                }

                client.SaveChanges();

                foreach (var relyingPartyAddress in relyingParty.RelyingPartyAddresses)
                {
                    relyingPartyAddress.RelyingPartyId = relyingParty.Id;
                    client.AddToRelyingPartyAddresses(relyingPartyAddress);
                }
                
                client.SaveChanges();
            }
            catch (DataServiceRequestException ex)
            {
                var baseException = ex.GetBaseException();
                if (!baseException.Message.ToUpperInvariant().Contains("ACS90013"))
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddRelyingPartyWithAsymmetricKey(string relyingPartyName, string realmAddress, string replyAddress, string errorAddress = "", TokenType tokenType = TokenType.SAML_2_0, int tokenLifetime = Constants.RelyingPartyTokenLifetime, byte[] signingCert = null, string signingCertPassword = "", byte[] encryptionCert = null, string[] ruleGroupNames = null, string[] allowedIdentityProviders = null)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                var defaultStartDate = DateTime.UtcNow;
                var defaultEndDate = defaultStartDate.AddYears(1);
                var asymmetricTokenEncryptionRequired = encryptionCert != null;

                RelyingParty relyingParty;
                CreateRelyingParty(client, relyingPartyName, ruleGroupNames, realmAddress, replyAddress, tokenType, errorAddress, tokenLifetime, asymmetricTokenEncryptionRequired, out relyingParty);

                if (signingCert != null)
                {
                    AddSigningKeyToRelyingParty(client, signingCert, signingCertPassword, defaultStartDate, defaultEndDate, relyingParty);
                }

                if (asymmetricTokenEncryptionRequired)
                {
                    AddEncryptionKeyToRelyingParty(client, encryptionCert, defaultStartDate, defaultEndDate, relyingParty);
                }

                client.SaveChanges();

                AddIdentityProviderToRelyingParty(client, allowedIdentityProviders, relyingParty);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddRelyingParty(string relyingPartyName, string realmAddress, string replyAddress, string errorAddress = "", TokenType tokenFormat = TokenType.SAML_2_0, int tokenLifetime = Constants.RelyingPartyTokenLifetime, byte[] signingCert = null, string signingCertPassword = null, byte[] encryptionCert = null, string[] ruleGroupName = null, string[] allowedIdentityProviders = null)
        {
            this.AddRelyingPartyWithAsymmetricKey(relyingPartyName, realmAddress, replyAddress, errorAddress, tokenFormat, tokenLifetime, signingCert, signingCertPassword, encryptionCert, ruleGroupNames: ruleGroupName, allowedIdentityProviders: allowedIdentityProviders);
        }

        public void RemoveRelyingParty(string relyingPartyName)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                RelyingParty relyingParty = client.RelyingParties
                                                  .Where(m => m.Name == relyingPartyName)
                                                  .FirstOrDefault();

                if (relyingParty != null)
                {
                    client.DeleteObject(relyingParty);
                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public RelyingParty RetrieveRelyingParty(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.RelyingParties
                    .Expand("RelyingPartyAddresses/RelyingParty,RelyingPartyIdentityProviders/IdentityProvider,RelyingPartyIdentityProviders/IdentityProvider/Issuer,RelyingPartyIdentityProviders/RelyingParty,RelyingPartyKeys/RelyingParty,RelyingPartyRuleGroups/RelyingParty,RelyingPartyRuleGroups/RuleGroup/Rules")
                    .Where(rp => rp.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<RelyingParty> RetrieveRelyingParties()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.RelyingParties.Expand("RelyingPartyKeys,RelyingPartyAddresses,RelyingPartyIdentityProviders,RelyingPartyRuleGroups/RuleGroup");

                return RetrieveAllResults<RelyingParty>(client, query as DataServiceQuery<RelyingParty>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddServiceKey(ServiceKey serviceKey, string relyingPartyName)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                if (serviceKey.IsPrimary)
                {
                    var currentPrimaryKey = client.ServiceKeys.Where(
                        k => k.IsPrimary &&
                        k.Type.Equals(serviceKey.Type, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (currentPrimaryKey != null)
                    {
                        currentPrimaryKey.IsPrimary = false;
                        client.UpdateObject(currentPrimaryKey);
                    }
                }

                client.AddToServiceKeys(serviceKey);
                client.SaveChanges();

                if (!string.IsNullOrEmpty(relyingPartyName))
                {
                    var relyingParty = this.RetrieveRelyingParty(relyingPartyName);

                    if (relyingParty != null)
                    {
                        var keyUsage = (KeyUsage)Enum.Parse(typeof(KeyUsage), serviceKey.Usage, true);
                        var keyType = (KeyType)Enum.Parse(typeof(KeyType), serviceKey.Type, true);

                        if (keyUsage == KeyUsage.Signing)
                        {
                            switch (keyType)
                            {
                                case KeyType.Symmetric:
                                    AddSigningKeyToRelyingParty(client, serviceKey.Value, serviceKey.StartDate, serviceKey.EndDate, relyingParty, serviceKey.DisplayName);
                                    break;

                                case KeyType.X509Certificate:
                                    AddSigningKeyToRelyingParty(client, serviceKey.Value, Convert.ToBase64String(serviceKey.Password), serviceKey.StartDate, serviceKey.EndDate, relyingParty, serviceKey.DisplayName);
                                    break;
                            }
                        }
                        else if (keyUsage == KeyUsage.Encrypting)
                        {
                            AddEncryptionKeyToRelyingParty(client, serviceKey.Value, serviceKey.StartDate, serviceKey.EndDate, relyingParty, serviceKey.DisplayName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddServiceKey(string displayName, string keyValue, string protectionPassword, KeyType keyType, KeyUsage keyUsage, DateTime? startDate = null, DateTime? endDate = null, bool isPrimary = true, string relyingPartyName = "")
        {
            this.AddServiceKey(displayName, Convert.FromBase64String(keyValue), protectionPassword, keyType, keyUsage, startDate, endDate, isPrimary, relyingPartyName);
        }

        public void AddServiceKey(string displayName, byte[] keyValue, string protectionPassword, KeyType keyType, KeyUsage keyUsage, DateTime? startDate = null, DateTime? endDate = null, bool isPrimary = true, string relyingPartyName = "")
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var defaultStartDate = DateTime.UtcNow;
                var defaultEndDate = defaultStartDate.AddYears(1);

                startDate = !startDate.HasValue || startDate.Value == DateTime.MinValue ? defaultStartDate : startDate.Value;
                endDate = !endDate.HasValue || endDate.Value == DateTime.MinValue ? defaultEndDate : endDate.Value;

                var serviceKey = new ServiceKey()
                {
                    DisplayName = displayName,
                    Type = keyType.ToString(),
                    Usage = keyUsage.ToString(),
                    Value = keyValue,
                    Password = string.IsNullOrEmpty(protectionPassword) ? null : new UTF8Encoding().GetBytes(protectionPassword),
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    IsPrimary = isPrimary
                };

                if (isPrimary)
                {
                    var currentPrimaryKey = client.ServiceKeys.Where(
                        k => k.IsPrimary &&
                        k.Type == keyType.ToString())
                        .FirstOrDefault();

                    if (currentPrimaryKey != null)
                    {
                        currentPrimaryKey.IsPrimary = false;
                        client.UpdateObject(currentPrimaryKey);
                    }
                }

                client.AddToServiceKeys(serviceKey);
                client.SaveChanges();

                if (!string.IsNullOrEmpty(relyingPartyName))
                {
                    var relyingParty = this.RetrieveRelyingParty(relyingPartyName);

                    if (relyingParty != null)
                    {
                        if (keyUsage == KeyUsage.Signing)
                        {
                            switch (keyType)
                            {
                                case KeyType.Symmetric:
                                    AddSigningKeyToRelyingParty(client, serviceKey.Value, serviceKey.StartDate, serviceKey.EndDate, relyingParty, displayName);
                                    break;

                                case KeyType.X509Certificate:
                                    AddSigningKeyToRelyingParty(client, serviceKey.Value, protectionPassword, serviceKey.StartDate, serviceKey.EndDate, relyingParty, displayName);
                                    break;
                            }
                        }
                        else if (keyUsage == KeyUsage.Encrypting)
                        {
                            AddEncryptionKeyToRelyingParty(client, serviceKey.Value, serviceKey.StartDate, serviceKey.EndDate, relyingParty, displayName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveServiceKey(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var serviceKeys = client.ServiceKeys.Where(e => e.DisplayName == name);

                if (serviceKeys.Count() != 0)
                {
                    foreach (ServiceKey key in serviceKeys)
                    {
                        client.DeleteObject(key);
                    }

                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<ServiceKey> RetrieveServiceKeys()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return RetrieveAllResults<ServiceKey>(client, client.ServiceKeys);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public ServiceKey RetrieveServiceKey(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.ServiceKeys
                    .Where(m => m.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddRuleGroup(RuleGroup ruleGroup)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                client.AddToRuleGroups(ruleGroup);
                client.SaveChanges();

                foreach (var rule in ruleGroup.Rules)
                {
                    this.AddRuleToRuleGroup(rule, ruleGroup.Name, rule.Issuer.Name);
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddRuleGroup(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                var ruleGroup = new RuleGroup()
                {
                    Name = name
                };

                client.AddToRuleGroups(ruleGroup);
                client.SaveChanges();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public string DefaultRuleGroupName()
        {
            return "Default Rule Group for AccessControlManagement";
        }

        public void RemoveAllRuleGroups()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                foreach (RuleGroup ruleGroup in client.RuleGroups.Where(ruleGroupToTest => ruleGroupToTest.Name != this.DefaultRuleGroupName()))
                {
                    client.DeleteObject(ruleGroup);
                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveRuleGroup(string name)
        {
            this.RemoveExistantRuleGroup(name, false);
        }

        public void RemoveExistantRuleGroup(string name, bool failIfRuleGroupDoesntExist = true)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                RuleGroup ruleGroup = client.RuleGroups.AddQueryOption("$filter", "Name eq '" + name + "'").FirstOrDefault();

                if (ruleGroup != null)
                {
                    client.DeleteObject(ruleGroup);
                    client.SaveChanges();
                }
                else if (failIfRuleGroupDoesntExist)
                {
                    throw new Exception("The rule group doesn't exist");
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public RuleGroup RetrieveRuleGroup(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.RuleGroups
                    .Expand("ConditionalRules,ConditionalRules/Issuer,RelyingPartyRuleGroups,RelyingPartyRuleGroups/RelyingParty,Rules,Rules/Issuer")
                    .Where(rg => rg.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<RuleGroup> RetrieveRuleGroups()
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                return RetrieveAllResults<RuleGroup>(client, client.RuleGroups);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<RuleGroup> RetrieveRuleGroupsOrderedByName()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.RuleGroups.OrderBy(ruleGroup => ruleGroup.Name);

                return RetrieveAllResults<RuleGroup>(client, query as DataServiceQuery<RuleGroup>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddSimpleRuleToRuleGroupWithoutSpecifyInputClaim(string ruleGroupName, string identityProviderName, string outputClaimType, string outputClaimValue)
        {
            this.AddSimpleRuleToRuleGroup(ruleGroupName, identityProviderName, outputClaimType: outputClaimType, outputClaimValue: outputClaimValue);
        }

        public void AddSimpleRuleToRuleGroup(string ruleGroupName, string identityProviderName, string inputClaimType = null, string inputClaimValue = null, string outputClaimType = null, string outputClaimValue = null, string description = "")
        {
            try
            {
                var rule = new Rule()
                {
                    InputClaimType = inputClaimType,
                    InputClaimValue = inputClaimValue,
                    OutputClaimType = outputClaimType,
                    OutputClaimValue = outputClaimValue,
                    Description = description
                };

                this.AddRuleToRuleGroup(rule, ruleGroupName, identityProviderName);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddPassthroughRuleToRuleGroup(string ruleGroupName, string identityProviderName, string inputClaimType = null, string outputClaimType = null, string description = "")
        {
            try
            {
                var rule = new Rule()
                {
                    InputClaimType = inputClaimType,
                    OutputClaimType = outputClaimType,
                    Description = description
                };

                this.AddRuleToRuleGroup(rule, ruleGroupName, identityProviderName);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<Rule> RetrieveRules(string ruleGroupName)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.Rules.Expand("RuleGroup,Issuer").Where(r => r.RuleGroup.Name == ruleGroupName);

                return RetrieveAllResults<Rule>(client, query as DataServiceQuery<Rule>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public Rule RetrieveRule(long id)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.Rules.Expand("RuleGroup,Issuer").Where(r => r.Id == id).Single();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveRule(Rule rule)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var ruleInContext = client.Rules.Where(r => r.Id == rule.Id).Single();
                client.DeleteObject(ruleInContext);
                client.SaveChanges();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveAllRulesInGroup(string ruleGroupName)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                RuleGroup ruleGroup = client.RuleGroups.Expand("Rules").AddQueryOption("$filter", "Name eq '" + ruleGroupName + "'").FirstOrDefault();

                if (ruleGroup != null)
                {
                    foreach (Rule rule in ruleGroup.Rules)
                    {
                        client.DeleteObject(rule);
                    }

                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveAllRulesInGroup(string ruleGroupName, string identityProviderName)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                RuleGroup ruleGroup = client.RuleGroups.Expand("Rules").AddQueryOption("$filter", "Name eq '" + ruleGroupName + "'").FirstOrDefault();

                if (ruleGroup != null)
                {
                    var identityProvider = this.RetrieveIdentityProvider(identityProviderName);
                    if (identityProvider != null)
                    {
                        foreach (Rule rule in ruleGroup.Rules.Where(r => r.IssuerId == identityProvider.IssuerId))
                        {
                            client.DeleteObject(rule);
                        }

                        client.SaveChanges(SaveChangesOptions.Batch);
                    }
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddServiceIdentity(ServiceIdentity serviceIdentity)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                client.AddToServiceIdentities(serviceIdentity);
                client.SaveChanges();

                foreach (var serviceIdentityKey in serviceIdentity.ServiceIdentityKeys)
                {
                    this.AddServiceIdentityKeyToServiceIdentity(serviceIdentityKey, serviceIdentity.Name);
                }

                foreach (var delegation in serviceIdentity.Delegations)
                {
                    this.AddDelegationToServiceIdentity(delegation, serviceIdentity.Name);
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddServiceIdentity(string name, string description)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                var serviceIdentity = new ServiceIdentity()
                {
                    Name = name,
                    Description = description
                };

                client.AddToServiceIdentities(serviceIdentity);
                client.SaveChanges();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveServiceIdentity(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var serviceIdentities = client.ServiceIdentities.Where(e => e.Name == name);

                if (serviceIdentities.Count() != 0)
                {
                    foreach (ServiceIdentity id in serviceIdentities)
                    {
                        client.DeleteObject(id);
                    }

                    client.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public ServiceIdentity RetrieveServiceIdentity(string name)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.ServiceIdentities.Expand("ServiceIdentityKeys,Delegations").Where(s => s.Name == name).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<ServiceIdentity> RetrieveServiceIdentities()
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.ServiceIdentities.Expand("ServiceIdentityKeys,Delegations");

                return RetrieveAllResults<ServiceIdentity>(client, query as DataServiceQuery<ServiceIdentity>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void AddServiceIdentityKeyToServiceIdentity(ServiceIdentityKey serviceIdentityKey, string serviceIdentityName)
        {
            var client = this.CreateManagementServiceClient();

            ServiceIdentity serviceIdentity = client.ServiceIdentities.AddQueryOption("$filter", "Name eq '" + serviceIdentityName + "'").FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new InvalidOperationException("Service Identity: " + serviceIdentityName + " does not exist");
            }

            serviceIdentityKey.ServiceIdentityId = serviceIdentity.Id;

            client.AddToServiceIdentityKeys(serviceIdentityKey);
            client.SaveChanges();
        }

        public void AddServiceIdentityKeyToServiceIdentity(string displayName, byte[] keyValue, IdentityKeyTypes keyType, IdentityKeyUsages keyUsage, string serviceIdentityName, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var defaultStartDate = DateTime.UtcNow;
                var defaultEndDate = defaultStartDate.AddYears(1);

                startDate = !startDate.HasValue || startDate.Value == DateTime.MinValue ? defaultStartDate : startDate.Value;
                endDate = !endDate.HasValue || endDate.Value == DateTime.MinValue ? defaultEndDate : endDate.Value;

                var serviceIdentityKey = new ServiceIdentityKey()
                {
                    DisplayName = displayName,
                    Type = keyType.ToString(),
                    Usage = keyUsage.ToString(),
                    Value = keyValue,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                };

                this.AddServiceIdentityKeyToServiceIdentity(serviceIdentityKey, serviceIdentityName);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public ServiceIdentityKey RetrieveServiceIdentityKey(long id)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                return client.ServiceIdentityKeys.Expand("ServiceIdentity").Where(s => s.Id == id).SingleOrDefault();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public IEnumerable<ServiceIdentityKey> RetrieveServiceIdentityKeys(string serviceIdentityName)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var query = client.ServiceIdentityKeys.Expand("ServiceIdentity").Where(r => r.ServiceIdentity.Name == serviceIdentityName);

                return RetrieveAllResults<ServiceIdentityKey>(client, query as DataServiceQuery<ServiceIdentityKey>);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        public void RemoveServiceIdentityKey(long id)
        {
            try
            {
                var client = this.CreateManagementServiceClient();
                var serviceIdentityKeyInContext = client.ServiceIdentityKeys.Where(r => r.Id == id).Single();
                client.DeleteObject(serviceIdentityKeyInContext);
                client.SaveChanges();
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }
        
        public void AddDelegationToServiceIdentity(Delegation delegation, string serviceIdentityName)
        {
            var client = this.CreateManagementServiceClient();

            ServiceIdentity serviceIdentity = client.ServiceIdentities.AddQueryOption("$filter", "Name eq '" + serviceIdentityName + "'").FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new InvalidOperationException("Service Identity: " + serviceIdentityName + " does not exist");
            }

            delegation.ServiceIdentityId = serviceIdentity.Id;

            client.AddToDelegations(delegation);
            client.SaveChanges();
        }

        public void AddRuleToRuleGroup(Rule rule, string ruleGroupName)
        {
            this.AddRuleToRuleGroup(rule, ruleGroupName, rule.Issuer.Name);
        }

        public void AddRuleToRuleGroup(Rule rule, string ruleGroupName, string identityProviderName)
        {
            var client = this.CreateManagementServiceClient();

            RuleGroup ruleGroup = client.RuleGroups.AddQueryOption("$filter", "Name eq '" + ruleGroupName + "'").FirstOrDefault();
            if (ruleGroup == null)
            {
                throw new InvalidOperationException("Rule Group: " + ruleGroupName + " does not exist");
            }

            Issuer issuer = null;

            if (!identityProviderName.Equals("LOCAL AUTHORITY", StringComparison.InvariantCultureIgnoreCase))
            {
                IdentityProvider identityProvider = this.RetrieveIdentityProvider(identityProviderName);

                if (identityProvider == null)
                {
                    throw new InvalidOperationException("Identity Provider: " + identityProviderName + " does not exist");
                }

                issuer = identityProvider.Issuer;
            }
            else
            {
                issuer = client.Issuers.AddQueryOption("$filter", "Name eq '" + "LOCAL AUTHORITY" + "'").FirstOrDefault();
            }


            rule.IssuerId = issuer.Id;
            rule.RuleGroupId = ruleGroup.Id;

            client.AddToRules(rule);
            client.SaveChanges();
        }

        public Issuer AddIdentityProvider(IdentityProvider identityProvider, string[] relyingParties)
        {
            try
            {
                var client = this.CreateManagementServiceClient();

                var oldIssuer = client.Issuers.Where(ip => ip.Name == identityProvider.Issuer.Name).FirstOrDefault();
                if (oldIssuer != null)
                {
                    client.DeleteObject(oldIssuer);
                }

                client.AddToIssuers(identityProvider.Issuer);
                client.SaveChanges(SaveChangesOptions.Batch);

                var oldIdentityProvider = client.IdentityProviders.Where(ip => ip.DisplayName.Equals(identityProvider.DisplayName, StringComparison.OrdinalIgnoreCase))
                                                                  .FirstOrDefault();
                if (oldIdentityProvider != null)
                {
                    client.DeleteObject(oldIdentityProvider);
                    client.SaveChanges();
                }

                identityProvider.IssuerId = identityProvider.Issuer.Id;
                client.AddToIdentityProviders(identityProvider);
                client.SaveChanges();

                foreach (var key in identityProvider.IdentityProviderKeys)
                {
                    key.IdentityProviderId = identityProvider.Id;
                    client.AddToIdentityProviderKeys(key);
                }
                
                client.SaveChanges(SaveChangesOptions.Batch);

                foreach (var address in identityProvider.IdentityProviderAddresses)
                {
                    address.IdentityProviderId = identityProvider.Id;
                    client.AddToIdentityProviderAddresses(address);
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                foreach (var claimType in identityProvider.IdentityProviderClaimTypes)
                {
                    var oldClaimType = client.ClaimTypes.Where(c => c.Uri.Equals(claimType.ClaimType.Uri)).FirstOrDefault();
                    if (oldClaimType != null)
                    {
                        client.DeleteObject(oldClaimType);
                        client.SaveChanges();
                    }

                    client.AddToClaimTypes(claimType.ClaimType);
                    client.SaveChanges(SaveChangesOptions.Batch);

                    claimType.ClaimTypeId = claimType.ClaimType.Id;
                    claimType.IdentityProviderId = identityProvider.Id;
                    client.AddToIdentityProviderClaimTypes(claimType);
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                if (relyingParties == null)
                {
                    relyingParties = client.RelyingParties.ToArray().Select(idp => idp.Name).ToArray();
                }

                foreach (var allowedRp in relyingParties)
                {
                    var rp = client.RelyingParties
                                    .Where(i => i.Name == allowedRp)
                                    .FirstOrDefault();

                    if (rp != null)
                    {
                        var rpidp = new RelyingPartyIdentityProvider
                        {
                            IdentityProviderId = identityProvider.Id,
                            RelyingPartyId = rp.Id
                        };

                        client.AddToRelyingPartyIdentityProviders(rpidp);
                    }
                }

                client.SaveChanges(SaveChangesOptions.Batch);

                return identityProvider.Issuer;
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }
        }

        private static void AddSigningKeyToRelyingParty(ManagementService client, byte[] signingCert, string signingCertPassword, DateTime defaultStartDate, DateTime defaultEndDate, RelyingParty relyingParty, string serviceKeyName = "", bool isPrimary = true)
        {
            var relyingPartyKey = new RelyingPartyKey()
            {
                DisplayName = string.IsNullOrEmpty(serviceKeyName) ? "Signing Certificate for " + relyingParty.DisplayName : serviceKeyName,
                Type = KeyType.X509Certificate.ToString(),
                Usage = KeyUsage.Signing.ToString(),
                Value = signingCert,
                Password = string.IsNullOrEmpty(signingCertPassword) ? null : new UTF8Encoding().GetBytes(signingCertPassword),
                RelyingPartyId = relyingParty.Id,
                StartDate = defaultStartDate,
                EndDate = defaultEndDate,
                IsPrimary = isPrimary
            };

            client.AddToRelyingPartyKeys(relyingPartyKey);
            client.SaveChanges();
        }

        private static void AddSigningKeyToRelyingParty(ManagementService client, byte[] symmetricKey, DateTime defaultStartDate, DateTime defaultEndDate, RelyingParty relyingParty, string serviceKeyName = "", bool isPrimary = true)
        {
            var relyingPartyKey = new RelyingPartyKey()
            {
                DisplayName = string.IsNullOrEmpty(serviceKeyName) ? "Signing Key for " + relyingParty.DisplayName : serviceKeyName,
                Type = KeyType.Symmetric.ToString(),
                Usage = KeyUsage.Signing.ToString(),
                Value = symmetricKey,
                RelyingPartyId = relyingParty.Id,
                StartDate = defaultStartDate,
                EndDate = defaultEndDate,
                IsPrimary = isPrimary
            };

            client.AddToRelyingPartyKeys(relyingPartyKey);
            client.SaveChanges();
        }

        private static void AddEncryptionKeyToRelyingParty(ManagementService client, byte[] encryptionCert, DateTime defaultStartDate, DateTime defaultEndDate, RelyingParty relyingParty, string serviceKeyName = "")
        {
            var relyingPartyKey = new RelyingPartyKey()
            {
                DisplayName = string.IsNullOrEmpty(serviceKeyName) ? "Encryption Certificate for " + relyingParty.DisplayName : serviceKeyName,
                Type = KeyType.X509Certificate.ToString(),
                Usage = KeyUsage.Encrypting.ToString(),
                Value = encryptionCert,
                RelyingPartyId = relyingParty.Id,
                StartDate = defaultStartDate,
                EndDate = defaultEndDate
            };

            client.AddToRelyingPartyKeys(relyingPartyKey);

            client.SaveChanges();
        }

        private static void AddIdentityProviderToRelyingParty(ManagementService client, string[] allowedIdentityProviders, RelyingParty relyingParty)
        {
            // if no allowed identity providers were set, allow all
            if (allowedIdentityProviders == null)
            {
                allowedIdentityProviders = client.IdentityProviders.ToList().Select(idp => idp.DisplayName).ToArray();
            }

            foreach (var allowedIdp in allowedIdentityProviders)
            {
                var idp = client.IdentityProviders
                                .Where(i => i.DisplayName.Equals(allowedIdp, StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

                if (idp != null)
                {
                    var rpidp = new RelyingPartyIdentityProvider
                    {
                        IdentityProviderId = idp.Id,
                        RelyingPartyId = relyingParty.Id
                    };

                    client.AddToRelyingPartyIdentityProviders(rpidp);
                    client.SaveChanges();
                }
            }
        }

        private static void CreateRelyingParty(ManagementService client, string relyingPartyName, string[] ruleGroupNames, string realmAddress, string replyAddress, TokenType tokenType, string errorAddress, int tokenLifetime, bool asymmetricTokenEncryptionRequired, out RelyingParty relyingParty)
        {
            // Create Relying Party
            relyingParty = new RelyingParty()
            {
                Name = relyingPartyName,
                DisplayName = relyingPartyName,
                Description = relyingPartyName,
                TokenType = tokenType.ToString(),
                TokenLifetime = tokenLifetime == 0 ? Constants.RelyingPartyTokenLifetime : tokenLifetime,
                AsymmetricTokenEncryptionRequired = asymmetricTokenEncryptionRequired
            };

            client.AddObject("RelyingParties", relyingParty);
            client.SaveChanges();

            foreach(var ruleGroupName in ruleGroupNames)
            {
                RuleGroup ruleGroup = client.RuleGroups.Where(rg => rg.Name.Equals(ruleGroupName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (ruleGroup == null)
                {
                    ruleGroup = new RuleGroup()
                    {
                        Name = ruleGroupName
                    };

                    client.AddToRuleGroups(ruleGroup);
                    client.SaveChanges();
                }

                var relyingPartyRuleGroup = new RelyingPartyRuleGroup
                {
                    RuleGroupId = ruleGroup.Id,
                    RelyingPartyId = relyingParty.Id
                };

                client.AddToRelyingPartyRuleGroups(relyingPartyRuleGroup);
            }

            // Create the Realm for Relying Party
            var realm = new RelyingPartyAddress()
            {
                Address = realmAddress,
                EndpointType = RelyingPartyAddressEndpointType.Realm.ToString(),
                RelyingPartyId = relyingParty.Id
            };

            client.AddToRelyingPartyAddresses(realm);

            if (!string.IsNullOrEmpty(replyAddress))
            {
                var reply = new RelyingPartyAddress()
                {
                    Address = replyAddress,
                    EndpointType = RelyingPartyAddressEndpointType.Reply.ToString(),
                    RelyingPartyId = relyingParty.Id
                };

                client.AddToRelyingPartyAddresses(reply);
            }

            if (!string.IsNullOrEmpty(errorAddress))
            {
                var reply = new RelyingPartyAddress()
                {
                    Address = errorAddress,
                    EndpointType = RelyingPartyAddressEndpointType.Error.ToString(),
                    RelyingPartyId = relyingParty.Id
                };

                client.AddToRelyingPartyAddresses(reply);
            }

            client.SaveChanges(SaveChangesOptions.Batch);
        }

        private static string GetAcsNamespace(string swtToken)
        {
            var identityProviderClaimValue = SwtTokenHelper.GetTokenProperties(swtToken, Uri.EscapeDataString(Constants.IdentityProviderClaimType));

            if (string.IsNullOrEmpty(identityProviderClaimValue))
            {
                throw new InvalidOperationException("Invalid SWT Token.\n\r" + swtToken);
            }
            
            var fullNamespace = new Uri(Uri.UnescapeDataString(identityProviderClaimValue));
            
            return fullNamespace.Authority.Split('.').First();
        }

        private static Regex CreateRegexForXmlNode(string xmlNode)
        {
            return new Regex(string.Format("<[^:]*:?{0}[ ]?[^>]*>(.*?)</[^:]*:?{0}>", xmlNode));
        }

        private static IEnumerable<T> RetrieveAllResults<T>(ManagementService client, DataServiceQuery<T> query)
        {
            var results = new List<T>();
            DataServiceQueryContinuation<T> token = null;
            var response = query.Execute() as QueryOperationResponse<T>;

            do
            {
                // If nextLink is not null, then there is a new page to load.
                if (token != null)
                {
                    // Load the new page from the next link URI.
                    response = client.Execute<T>(token) as QueryOperationResponse<T>;
                }

                // Enumerate the results in the response.
                foreach (T result in response)
                {
                    results.Add(result);
                }
            }
            while ((token = response.GetContinuation()) != null); // Get the next link, and continue while there is a next link.

            return results;
        }

        private string ImportWsFederationMetadata(byte[] fedMetadata, string metadataImporterEndpoint)
        {
            HttpWebRequest postRequest = (HttpWebRequest)WebRequest.Create(metadataImporterEndpoint);
            postRequest.Method = "POST";

            this.AttachTokenWithWritePermissions(postRequest);

            using (Stream postStream = postRequest.GetRequestStream())
            {
                for (int i = 0; i < fedMetadata.Length; i++)
                {
                    postStream.WriteByte(fedMetadata[i]);
                }
            }

            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)postRequest.GetResponse();
            }
            catch (WebException e)
            {
                resp = (HttpWebResponse)e.Response;

                string responseHtml;
                using (var stream = resp.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        responseHtml = reader.ReadToEnd();
                    }
                }

                throw new WebException(responseHtml, e);
            }

            var serializer = new MetadataSerializer();

            using (var memoryStream = new MemoryStream(fedMetadata))
            {
                var metadata = serializer.ReadMetadata(memoryStream) as Microsoft.IdentityModel.Protocols.WSFederation.Metadata.EntityDescriptor;
                return metadata.EntityId.Id;
            }
        }

        private string GetTokenFromACS()
        {
            var client = new WebClient();
            client.BaseAddress = string.Format("https://{0}.{1}", this._serviceNamespace, Constants.AcsHostName);

            var values = new NameValueCollection();
            values.Add("wrap_name", this._serviceIdentityUsernameForManagement);
            values.Add("wrap_password", this._serviceIdentityPasswordForManagement);
            values.Add("wrap_scope", string.Format("https://{0}.{1}/{2}", this._serviceNamespace, Constants.AcsHostName, Constants.ManagementServiceHead));

            var response = string.Empty;

            try
            {
                byte[] responseBytes = client.UploadValues("WRAPv0.9/", "POST", values);

                response = Encoding.UTF8.GetString(responseBytes);
            }
            catch (Exception ex)
            {
                throw this.TryGetExceptionDetails(ex);
            }

            // Extract the SWT token and return it.
            return response
                .Split('&')
                .Single(value => value.StartsWith("wrap_access_token=", StringComparison.OrdinalIgnoreCase))
                .Split('=')[1];
        }

        private Exception TryGetExceptionDetails(Exception ex)
        {
            if (ex is WebException)
            {
                var webEx = (WebException)ex;
                
                if (webEx.Response != null && webEx.Response is HttpWebResponse)
                {
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        return new InvalidCredentialException("The specified key is not valid for the specified namespace.", ex);
                    }
                }
            }

            if (ex is DataServiceRequestException)
            {
                var dataServiceEx = (DataServiceRequestException)ex;
                var message = dataServiceEx.Message;

                if (dataServiceEx.Response != null && dataServiceEx.Response.Count() > 0)
                {
                    var response = dataServiceEx.Response.Last();
                    var statusCode = (HttpStatusCode)response.StatusCode;

                    if (response.Error != null)
                    {
                        var nodeRegex = CreateRegexForXmlNode("message");
                        message = string.Concat(statusCode, ". ", nodeRegex.Match(response.Error.Message).Groups[1].Value);
                    }
                }

                new ServiceManagementException(message, dataServiceEx, this.MgmtSwtToken);
            }

            return
                string.IsNullOrEmpty(this.MgmtSwtToken) ?
                ex :
                new ServiceManagementException(ex, this.MgmtSwtToken);
        }

        private ManagementService CreateManagementServiceClient()
        {
            var tokenExpirationTime = SwtTokenHelper.GetExpiryTime(this.MgmtSwtToken);
            if (DateTime.UtcNow.AddMinutes(5).CompareTo(tokenExpirationTime) >= 0)
            {
                if (string.IsNullOrEmpty(this._serviceIdentityUsernameForManagement) || string.IsNullOrEmpty(this._serviceIdentityPasswordForManagement))
                {
                    throw new InvalidOperationException("The token has expired. Expiration Date: " + tokenExpirationTime + " (UTC).");
                }
                else
                {
                    this.MgmtSwtToken = this.GetTokenFromACS();
                }
            }

            var managementServiceEndpoint = string.Format("https://{0}.{1}/{2}", this._serviceNamespace, Constants.AcsHostName, Constants.ManagementServiceHead);
            var managementService = new ManagementService(new Uri(managementServiceEndpoint));

            managementService.SendingRequest += (o, args) =>
            {
                AttachTokenWithWritePermissions(args.Request);
            };

            return managementService;
        }

        private void AttachTokenWithWritePermissions(WebRequest request)
        {
            request.Headers.Add(HttpRequestHeader.Authorization, "WRAP access_token=\"" + Uri.UnescapeDataString(this.MgmtSwtToken) + "\"");
        }
    }
}