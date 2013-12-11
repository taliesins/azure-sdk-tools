using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    using WrapperClaimType = Microsoft.WindowsAzure.Commands.ACS.Service.ClaimType;
    using WrapperDelegation = Microsoft.WindowsAzure.Commands.ACS.Service.Delegation;
    using WrapperIdentityProvider = Microsoft.WindowsAzure.Commands.ACS.Service.IdentityProvider;
    using WrapperIdentityProviderAddress = Microsoft.WindowsAzure.Commands.ACS.Service.IdentityProviderAddress;
    using WrapperIdentityProviderClaimType = Microsoft.WindowsAzure.Commands.ACS.Service.IdentityProviderClaimType;
    using WrapperIdentityProviderKey = Microsoft.WindowsAzure.Commands.ACS.Service.IdentityProviderKey;
    using WrapperIssuer = Microsoft.WindowsAzure.Commands.ACS.Service.Issuer;
    using WrapperRelyingParty = Microsoft.WindowsAzure.Commands.ACS.Service.RelyingParty;
    using WrapperRelyingPartyAddress = Microsoft.WindowsAzure.Commands.ACS.Service.RelyingPartyAddress;
    using WrapperRelyingPartyIdentityProvider = Microsoft.WindowsAzure.Commands.ACS.Service.RelyingPartyIdentityProvider;
    using WrapperRelyingPartyKey = Microsoft.WindowsAzure.Commands.ACS.Service.RelyingPartyKey;
    using WrapperRelyingPartyRuleGroup = Microsoft.WindowsAzure.Commands.ACS.Service.RelyingPartyRuleGroup;
    using WrapperRule = Microsoft.WindowsAzure.Commands.ACS.Service.Rule;
    using WrapperRuleGroup = Microsoft.WindowsAzure.Commands.ACS.Service.RuleGroup;
    using WrapperServiceIdentity = Microsoft.WindowsAzure.Commands.ACS.Service.ServiceIdentity;
    using WrapperServiceIdentityKey = Microsoft.WindowsAzure.Commands.ACS.Service.ServiceIdentityKey;
    using WrapperServiceKey = Microsoft.WindowsAzure.Commands.ACS.Service.ServiceKey;

    public static class ModelExtensions
    {
        public static Rule ToModel(this WrapperRule rule, string mgmtTopken)
        {
            var identityProviderName = string.Empty;
            if (rule.Issuer != null)
            {
                identityProviderName = rule.Issuer.IdentityProviders != null && rule.Issuer.IdentityProviders.Count() > 0 ? rule.Issuer.IdentityProviders.First().DisplayName : rule.Issuer.Name;
            }

            return
                rule == null ?
                null :
                new Rule(mgmtTopken)
                {
                    Id = rule.Id,
                    Description = rule.Description,
                    IdentityProviderName = identityProviderName,
                    InputClaimType = rule.InputClaimType,
                    InputClaimValue = rule.InputClaimValue,
                    OutputClaimType = rule.OutputClaimType,
                    OutputClaimValue = rule.OutputClaimValue,
                    SystemReserved = rule.SystemReserved
                };
        }

        public static RuleGroup ToModel(this WrapperRuleGroup ruleGroup, string mgmtTopken)
        {
            return
                ruleGroup == null ?
                null :
                new RuleGroup(mgmtTopken)
                {
                    Id = ruleGroup.Id,
                    Name = ruleGroup.Name,
                    Rules = ruleGroup.Rules != null ? ruleGroup.Rules.Select(r => r.ToModel(mgmtTopken)).ToArray() : null,
                    SystemReserved = ruleGroup.SystemReserved
                };
        }

        public static ServiceKey ToModel(this WrapperServiceKey serviceKey, string mgmtTopken)
        {
            return
                serviceKey == null ?
                null :
                new ServiceKey(mgmtTopken)
                {
                    Id = serviceKey.Id,
                    Name = serviceKey.DisplayName,
                    IsPrimary = serviceKey.IsPrimary,
                    Type = serviceKey.Type,
                    Usage = serviceKey.Usage,
                    Value = serviceKey.Value,
                    Password = serviceKey.Password != null ? Convert.ToBase64String(serviceKey.Password) : null,
                    StartDate = serviceKey.StartDate,
                    EndDate = serviceKey.EndDate,
                    SystemReserved = serviceKey.SystemReserved
                };
        }

        public static ServiceKey ToModel(this WrapperIdentityProviderKey identityProviderKey, string mgmtTopken)
        {
            return
                identityProviderKey == null ?
                null :
                new ServiceKey(mgmtTopken)
                {
                    Id = identityProviderKey.Id,
                    Name = identityProviderKey.DisplayName,
                    IsPrimary = null,
                    Type = identityProviderKey.Type,
                    Usage = identityProviderKey.Usage,
                    Value = identityProviderKey.Value,
                    Password = identityProviderKey.Password != null ? Convert.ToBase64String(identityProviderKey.Password) : null,
                    StartDate = identityProviderKey.StartDate,
                    EndDate = identityProviderKey.EndDate,
                    SystemReserved = identityProviderKey.SystemReserved
                };
        }

        public static ServiceKey ToModel(this WrapperRelyingPartyKey relyingPartyKey, string mgmtTopken)
        {
            return
                relyingPartyKey == null ?
                null :
                new ServiceKey(mgmtTopken)
                {
                    Id = relyingPartyKey.Id,
                    Name = relyingPartyKey.DisplayName,
                    IsPrimary = relyingPartyKey.IsPrimary,
                    Type = relyingPartyKey.Type,
                    Usage = relyingPartyKey.Usage,
                    Value = relyingPartyKey.Value,
                    Password = relyingPartyKey.Password != null ? Convert.ToBase64String(relyingPartyKey.Password) : null,
                    StartDate = relyingPartyKey.StartDate,
                    EndDate = relyingPartyKey.EndDate,
                    SystemReserved = relyingPartyKey.SystemReserved
                };
        }

        public static RelyingParty ToModel(this WrapperRelyingParty relyingParty, string mgmtTopken)
        {
            var result = relyingParty == null
                ? null
                : new RelyingParty(mgmtTopken)
                {
                    Id = relyingParty.Id,
                    Name = relyingParty.Name,
                    AsymmetricTokenEncryptionRequired = relyingParty.AsymmetricTokenEncryptionRequired,
                    Realm = relyingParty.RelyingPartyAddresses != null ? relyingParty.RelyingPartyAddresses.GetAddress(RelyingPartyAddressEndpointType.Realm) : string.Empty,
                    ReturnUrl = relyingParty.RelyingPartyAddresses != null ? relyingParty.RelyingPartyAddresses.GetAddress(RelyingPartyAddressEndpointType.Reply) : string.Empty,
                    ErrorUrl = relyingParty.RelyingPartyAddresses != null ? relyingParty.RelyingPartyAddresses.GetAddress(RelyingPartyAddressEndpointType.Error) : string.Empty,
                    TokenType = relyingParty.TokenType,
                    TokenLifetime = relyingParty.TokenLifetime,
                    IdentityProviders = relyingParty.RelyingPartyIdentityProviders != null ? relyingParty.RelyingPartyIdentityProviders.Select(rpip => rpip.IdentityProvider.ToModel(mgmtTopken)).ToArray() : null,
                    Keys = relyingParty.RelyingPartyKeys != null ? relyingParty.RelyingPartyKeys.Select(k => k.ToModel(mgmtTopken)).ToArray() : null,
                    RuleGroups = relyingParty.RelyingPartyRuleGroups != null ? relyingParty.RelyingPartyRuleGroups.Select(rg => rg.RuleGroup.ToModel(mgmtTopken)).ToArray() : null,
                    SystemReserved = relyingParty.SystemReserved
                };

            return result;
        }

        public static IdentityProvider ToModel(this WrapperIdentityProvider identityProvider, string mgmtTopken)
        {
            return
                identityProvider == null ?
                null :
                new IdentityProvider(mgmtTopken)
                {
                    Id = identityProvider.Id,
                    Name = identityProvider.DisplayName,
                    Description = identityProvider.Description,
                    Realm = identityProvider.Realm,
                    WebSSOProtocolType = identityProvider.WebSSOProtocolType,
                    LoginLinkName = identityProvider.LoginLinkName,
                    LoginParameters = identityProvider.LoginParameters,
                    IssuerName = identityProvider.Issuer.Name,
                    SignInEndpoint = identityProvider.IdentityProviderAddresses != null ? identityProvider.IdentityProviderAddresses.GetAddress(EndpointType.SignIn) : string.Empty,
                    SignOutEndpoint = identityProvider.IdentityProviderAddresses != null ? identityProvider.IdentityProviderAddresses.GetAddress(EndpointType.SignOut) : string.Empty,
                    EmailDomain = identityProvider.IdentityProviderAddresses != null ? identityProvider.IdentityProviderAddresses.GetAddress(EndpointType.EmailDomain) : string.Empty,
                    ImageUrl = identityProvider.IdentityProviderAddresses != null ? identityProvider.IdentityProviderAddresses.GetAddress(EndpointType.ImageUrl) : string.Empty,
                    FedMetadataUrl = identityProvider.IdentityProviderAddresses != null ? identityProvider.IdentityProviderAddresses.GetAddress(EndpointType.FedMetadataUrl) : string.Empty,
                    ClaimTypes = identityProvider.IdentityProviderClaimTypes != null ? identityProvider.IdentityProviderClaimTypes.Select(ct => ct.ClaimType.Uri).ToArray() : null,
                    Keys = identityProvider.IdentityProviderKeys != null ? identityProvider.IdentityProviderKeys.Select(k => k.ToModel(mgmtTopken)).ToArray() : null,
                    RelyingParties = identityProvider.RelyingPartyIdentityProviders != null ? identityProvider.RelyingPartyIdentityProviders.Select(rpip => rpip.RelyingParty.ToModel(mgmtTopken)).ToArray() : null,
                    SystemReserved = identityProvider.SystemReserved
                };
        }

        public static ServiceIdentity ToModel(this WrapperServiceIdentity serviceIdentity, string mgmtTopken)
        {
            return
                serviceIdentity == null ?
                null :
                new ServiceIdentity(mgmtTopken)
                {
                    Id = serviceIdentity.Id,
                    Name = serviceIdentity.Name,
                    Description = serviceIdentity.Description,
                    RedirectAddress = serviceIdentity.RedirectAddress,
                    ServiceIdentityKeys = serviceIdentity.ServiceIdentityKeys != null ? serviceIdentity.ServiceIdentityKeys.Select(sik => sik.ToModel(mgmtTopken)).ToArray() : null,
                    Delegations = serviceIdentity.Delegations != null ? serviceIdentity.Delegations.Select(d => d.ToModel(mgmtTopken)).ToArray() : null
                };
        }

        public static ServiceIdentityKey ToModel(this WrapperServiceIdentityKey serviceIdentityKey, string mgmtTopken)
        {
            return
                serviceIdentityKey == null ?
                null :
                new ServiceIdentityKey(mgmtTopken)
                {
                    Id = serviceIdentityKey.Id,
                    Name = serviceIdentityKey.DisplayName,
                    StartDate = serviceIdentityKey.StartDate,
                    EndDate = serviceIdentityKey.EndDate,
                    Type = serviceIdentityKey.Type,
                    Usage = serviceIdentityKey.Usage,
                    Value = serviceIdentityKey.Value
                };
        }

        public static Delegation ToModel(this WrapperDelegation delegation, string mgmtTopken)
        {
            return
                delegation == null ?
                null :
                new Delegation(mgmtTopken)
                {
                    Id = delegation.Id,
                    IdentityProvider = delegation.IdentityProvider,
                    NameIdentifier = delegation.NameIdentifier,
                    Permissions = delegation.Permissions,
                    AuthorizationCode = delegation.AuthorizationCode,
                    RelyingParty = delegation.RelyingParty.ToModel(mgmtTopken)
                };
        }

        public static string GetAddress(this Collection<WrapperIdentityProviderAddress> identityProviderAddresses, EndpointType endpointType)
        {
            return identityProviderAddresses.Any(a => a.EndpointType.Equals(endpointType.ToString(), StringComparison.OrdinalIgnoreCase)) ?
                identityProviderAddresses.First(a => a.EndpointType.Equals(endpointType.ToString(), StringComparison.OrdinalIgnoreCase)).Address :
                string.Empty;
        }

        public static string GetAddress(this Collection<WrapperRelyingPartyAddress> relyingPartyAddresses, RelyingPartyAddressEndpointType endpointType)
        {
            return relyingPartyAddresses.Any(a => a.EndpointType.Equals(endpointType.ToString(), StringComparison.OrdinalIgnoreCase)) ?
                relyingPartyAddresses.First(a => a.EndpointType.Equals(endpointType.ToString(), StringComparison.OrdinalIgnoreCase)).Address :
                string.Empty;
        }

        public static WrapperRelyingParty ToWrapperModel(this RelyingParty relyingParty)
        {
            var result = new WrapperRelyingParty
            {
                AsymmetricTokenEncryptionRequired = relyingParty.AsymmetricTokenEncryptionRequired,
                Description = relyingParty.Name,
                DisplayName = relyingParty.Name,
                Name = relyingParty.Name,
                TokenLifetime = relyingParty.TokenLifetime,
                TokenType = relyingParty.TokenType,
            };

            var addresses = new[]
            {
                ExtractRealmRelyingPartyAddress(relyingParty.Realm, result),
                ExtractReturnRelyingPartyAddress(relyingParty.ReturnUrl, result),
                ExtractErrorRelyingPartyAddress(relyingParty.ErrorUrl, result)
            };
            result.RelyingPartyAddresses = new Collection<WrapperRelyingPartyAddress>(addresses.ToList());

            result.RelyingPartyIdentityProviders = relyingParty.IdentityProviders != null
                ? new Collection<WrapperRelyingPartyIdentityProvider>(relyingParty.IdentityProviders.Select(p => p.ToWrapperModel(result)).ToList())
                : new Collection<WrapperRelyingPartyIdentityProvider>();

            result.RelyingPartyKeys = relyingParty.Keys != null
                ? new Collection<WrapperRelyingPartyKey>(relyingParty.Keys.Select(k => k.ToWrapperModel(result)).ToList())
                : new Collection<WrapperRelyingPartyKey>();

            result.RelyingPartyRuleGroups = relyingParty.RuleGroups != null
                ? new Collection<WrapperRelyingPartyRuleGroup>(relyingParty.RuleGroups.Select(g => g.ToWrapperModel(result)).ToList())
                : new Collection<WrapperRelyingPartyRuleGroup>();

            return result;
        }

        public static WrapperRelyingPartyIdentityProvider ToWrapperModel(this IdentityProvider identityProvider, WrapperRelyingParty relyingParty)
        {
            return new WrapperRelyingPartyIdentityProvider
            {
                IdentityProvider = identityProvider.ToWrapperModel(),
                RelyingParty = relyingParty
            };
        }

        public static WrapperRelyingPartyKey ToWrapperModel(this ServiceKey serviceKey, WrapperRelyingParty relyingParty)
        {
            return new WrapperRelyingPartyKey
            {
                DisplayName = serviceKey.Name,
                EndDate = serviceKey.EndDate,
                IsPrimary = serviceKey.IsPrimary.HasValue ? serviceKey.IsPrimary.Value : false,
                Password = serviceKey.Password != null ? Convert.FromBase64String(serviceKey.Password) : null,
                RelyingParty = relyingParty,
                StartDate = serviceKey.StartDate,
                Type = serviceKey.Type,
                Usage = serviceKey.Usage,
                Value = serviceKey.Value
            };
        }

        public static WrapperRuleGroup ToWrapperModel(this RuleGroup ruleGroup)
        {
            var result = new WrapperRuleGroup
            {
                Name = ruleGroup.Name,
            };

            if (ruleGroup.Rules != null)
            {
                result.Rules = new Collection<WrapperRule>(ruleGroup.Rules.Select(r => r.ToWrapperModel(result)).ToList());
            }

            return result;
        }

        public static WrapperRelyingPartyRuleGroup ToWrapperModel(this RuleGroup ruleGroup, WrapperRelyingParty relyingParty)
        {
            var relyingPartyRuleGroup = new WrapperRelyingPartyRuleGroup
            {
                RelyingParty = relyingParty,
                RuleGroup = ruleGroup != null ? ruleGroup.ToWrapperModel() : null,
            };

            return relyingPartyRuleGroup;
        }

        public static WrapperRule ToWrapperModel(this Rule rule, WrapperRuleGroup ruleGroup)
        {
            return new WrapperRule
            {
                Description = rule.Description,
                InputClaimType = rule.InputClaimType,
                InputClaimValue = rule.InputClaimValue,
                Issuer = new WrapperIssuer { Name = rule.IdentityProviderName },
                OutputClaimType = rule.OutputClaimType,
                OutputClaimValue = rule.OutputClaimValue,
                RuleGroup = ruleGroup
            };
        }

        public static WrapperRule ToWrapperModel(this Rule rule)
        {
            return new WrapperRule
            {
                Description = rule.Description,
                InputClaimType = rule.InputClaimValue,
                InputClaimValue = rule.InputClaimValue,
                OutputClaimType = rule.OutputClaimType,
                OutputClaimValue = rule.OutputClaimValue,
                Issuer = new WrapperIssuer { Name = rule.IdentityProviderName }
            };
        }

        public static WrapperIdentityProvider ToWrapperModel(this IdentityProvider identityProvider)
        {
            var result = new WrapperIdentityProvider
            {
                Description = identityProvider.Description,
                DisplayName = identityProvider.Name,
                Issuer = new WrapperIssuer
                {
                    Name = !string.IsNullOrEmpty(identityProvider.IssuerName)
                        ? identityProvider.IssuerName
                        : identityProvider.Name
                },
                LoginLinkName = identityProvider.LoginLinkName,
                LoginParameters = identityProvider.LoginParameters,
                Realm = identityProvider.Realm,
                WebSSOProtocolType = identityProvider.WebSSOProtocolType
            };

            result.IdentityProviderClaimTypes = identityProvider.ClaimTypes != null
                ? new Collection<WrapperIdentityProviderClaimType>(identityProvider.ClaimTypes.Select(c => new WrapperIdentityProviderClaimType { ClaimType = new WrapperClaimType { Uri = c }, IdentityProvider = result }).ToList())
                : new Collection<WrapperIdentityProviderClaimType>();

            result.IdentityProviderAddresses = new Collection<WrapperIdentityProviderAddress>();

            if (!string.IsNullOrEmpty(identityProvider.SignInEndpoint))
            {
                result.IdentityProviderAddresses.Add(ExtractSignInIdentityProviderAddress(result, identityProvider.SignInEndpoint));
            }

            if (!string.IsNullOrEmpty(identityProvider.SignOutEndpoint))
            {
                result.IdentityProviderAddresses.Add(ExtractSignOutIdentityProviderAddress(result, identityProvider.SignOutEndpoint));
            }

            if (!string.IsNullOrEmpty(identityProvider.FedMetadataUrl))
            {
                result.IdentityProviderAddresses.Add(ExtractFedMetadataIdentityProviderAddress(result, identityProvider.FedMetadataUrl));
            }

            if (!string.IsNullOrEmpty(identityProvider.ImageUrl))
            {
                result.IdentityProviderAddresses.Add(ExtractImageIdentityProviderAddress(result, identityProvider.ImageUrl));
            }

            if (!string.IsNullOrEmpty(identityProvider.EmailDomain))
            {
                result.IdentityProviderAddresses.Add(ExtractEmailIdentityProviderAddress(result, identityProvider.EmailDomain));
            }

            result.IdentityProviderKeys = identityProvider.Keys != null
                ? new Collection<WrapperIdentityProviderKey>(identityProvider.Keys.Select(k => k.ToWrapperModel(result)).ToList())
                : new Collection<WrapperIdentityProviderKey>();

            return result;
        }

        public static WrapperIdentityProviderKey ToWrapperModel(this ServiceKey serviceKey, WrapperIdentityProvider identityProvider)
        {
            var key = new WrapperIdentityProviderKey
            {
                IdentityProvider = identityProvider,
                DisplayName = serviceKey.Name,
                StartDate = serviceKey.StartDate,
                EndDate = serviceKey.EndDate,
                Type = serviceKey.Type,
                Usage = serviceKey.Usage,
                Value = serviceKey.Value
            };

            return key;
        }

        public static WrapperServiceKey ToWrapperModel(this ServiceKey serviceKey)
        {
            return new WrapperServiceKey
            {
                DisplayName = serviceKey.Name,
                EndDate = serviceKey.EndDate,
                IsPrimary = serviceKey.IsPrimary.HasValue ? serviceKey.IsPrimary.Value : false,
                Password = serviceKey.Password != null ? Convert.FromBase64String(serviceKey.Password) : null,
                StartDate = serviceKey.StartDate,
                Type = serviceKey.Type,
                Usage = serviceKey.Usage,
                Value = serviceKey.Value,
            };
        }

        public static WrapperServiceIdentity ToWrapperModel(this ServiceIdentity serviceIdentity)
        {
            var result = new WrapperServiceIdentity
            {
                Name = serviceIdentity.Name,
                Description = serviceIdentity.Description,
                RedirectAddress = serviceIdentity.RedirectAddress
            };

            result.Delegations = serviceIdentity.Delegations != null
                ? new Collection<WrapperDelegation>(serviceIdentity.Delegations.Select(d => d.ToWrapperModel()).ToList())
                : new Collection<WrapperDelegation>();

            result.ServiceIdentityKeys = serviceIdentity.ServiceIdentityKeys != null
                ? new Collection<WrapperServiceIdentityKey>(serviceIdentity.ServiceIdentityKeys.Select(d => d.ToWrapperModel()).ToList())
                : new Collection<WrapperServiceIdentityKey>();

            return result;
        }

        public static WrapperDelegation ToWrapperModel(this Delegation delegation)
        {
            return new WrapperDelegation
            {
                AuthorizationCode = delegation.AuthorizationCode,
                IdentityProvider = delegation.IdentityProvider,
                NameIdentifier = delegation.NameIdentifier,
                Permissions = delegation.Permissions,
                RelyingParty = delegation.RelyingParty.ToWrapperModel()
            };
        }

        public static WrapperServiceIdentityKey ToWrapperModel(this ServiceIdentityKey serviceIdentityKey)
        {
            return new WrapperServiceIdentityKey
            {
                DisplayName = serviceIdentityKey.Name,
                EndDate = serviceIdentityKey.EndDate,
                StartDate = serviceIdentityKey.StartDate,
                Type = serviceIdentityKey.Type,
                Usage = serviceIdentityKey.Usage,
                Value = serviceIdentityKey.Value
            };
        }

        private static WrapperIdentityProviderAddress ExtractSignInIdentityProviderAddress(WrapperIdentityProvider result, string endpoint)
        {
            return new WrapperIdentityProviderAddress
            {
                IdentityProvider = result,
                EndpointType = EndpointType.SignIn.ToString(),
                Address = endpoint
            };
        }

        private static WrapperIdentityProviderAddress ExtractSignOutIdentityProviderAddress(WrapperIdentityProvider result, string endpoint)
        {
            return new WrapperIdentityProviderAddress
            {
                IdentityProvider = result,
                EndpointType = EndpointType.SignOut.ToString(),
                Address = endpoint
            };
        }

        private static WrapperIdentityProviderAddress ExtractFedMetadataIdentityProviderAddress(WrapperIdentityProvider result, string endpoint)
        {
            return new WrapperIdentityProviderAddress
            {
                IdentityProvider = result,
                EndpointType = EndpointType.FedMetadataUrl.ToString(),
                Address = endpoint
            };
        }

        private static WrapperIdentityProviderAddress ExtractImageIdentityProviderAddress(WrapperIdentityProvider result, string endpoint)
        {
            return new WrapperIdentityProviderAddress
            {
                IdentityProvider = result,
                EndpointType = EndpointType.ImageUrl.ToString(),
                Address = endpoint
            };
        }

        private static WrapperIdentityProviderAddress ExtractEmailIdentityProviderAddress(WrapperIdentityProvider result, string endpoint)
        {
            return new WrapperIdentityProviderAddress
            {
                IdentityProvider = result,
                EndpointType = EndpointType.EmailDomain.ToString(),
                Address = endpoint
            };
        }

        private static WrapperRelyingPartyAddress ExtractRealmRelyingPartyAddress(string address, WrapperRelyingParty relyingParty)
        {
            return new WrapperRelyingPartyAddress()
            {
                Address = address,
                EndpointType = RelyingPartyAddressEndpointType.Realm.ToString(),
                RelyingParty = relyingParty
            };
        }

        private static WrapperRelyingPartyAddress ExtractReturnRelyingPartyAddress(string address, WrapperRelyingParty relyingParty)
        {
            return new WrapperRelyingPartyAddress()
            {
                Address = address,
                EndpointType = RelyingPartyAddressEndpointType.Reply.ToString(),
                RelyingParty = relyingParty
            };
        }

        private static WrapperRelyingPartyAddress ExtractErrorRelyingPartyAddress(string address, WrapperRelyingParty relyingParty)
        {
            return new WrapperRelyingPartyAddress()
            {
                Address = address,
                EndpointType = RelyingPartyAddressEndpointType.Error.ToString(),
                RelyingParty = relyingParty
            };
        }
    }
}
