using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Commands.ACS.Model
{
    [Serializable]
    public class ServiceNamespace
    {
        public IdentityProvider[] IdentityProviders { get; set; }

        public RelyingParty[] RelyingParties { get; set; }

        public RuleGroup[] RuleGroups { get; set; }

        public ServiceKey[] ServiceKeys { get; set; }

        public ServiceIdentity[] ServiceIdentities { get; set; }

        public void Serialize(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                var serializer = new XmlSerializer(typeof(ServiceNamespace));
                serializer.Serialize(writer, this);
            }
        }

        public void Deserialize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "The {0} file does not exist.", filePath),
                    "filePath");
            }

            using (var reader = new StreamReader(filePath))
            {
                var serializer = new XmlSerializer(typeof(ServiceNamespace));
                var serviceNamespace = (ServiceNamespace)serializer.Deserialize(reader);

                this.IdentityProviders = serviceNamespace.IdentityProviders;
                this.RelyingParties = serviceNamespace.RelyingParties;
                this.RuleGroups = serviceNamespace.RuleGroups;
                this.ServiceKeys = serviceNamespace.ServiceKeys;
                this.ServiceIdentities = serviceNamespace.ServiceIdentities;
            }
        }
    }
}
