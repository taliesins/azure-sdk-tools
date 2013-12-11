﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.ServiceManagement;

namespace Microsoft.WindowsAzure.Commands.Utilities.ServiceBus
{
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ExtendedServiceBusNamespace
    {
        private static Regex connectionStringRegex = new Regex(@"Endpoint=(.*);SharedSecretIssuer=(.*);SharedSecretValue=(.*)");
        public ExtendedServiceBusNamespace()
        {

        }

        public ExtendedServiceBusNamespace(
            ServiceBusNamespace serviceBusNamespace,
            IList<NamespaceDescription> descriptions)
        {
            Name = serviceBusNamespace.Name;

            Region = serviceBusNamespace.Region;

            Status = serviceBusNamespace.Status;

            CreatedAt = serviceBusNamespace.CreatedAt;

            AcsManagementEndpoint = serviceBusNamespace.AcsManagementEndpoint != null ? serviceBusNamespace.AcsManagementEndpoint.ToString() : string.Empty;

            ServiceBusEndpoint = serviceBusNamespace.ServiceBusEndpoint != null ? serviceBusNamespace.ServiceBusEndpoint.ToString() : string.Empty;

            if (descriptions != null && descriptions.Count != 0)
            {
                NamespaceDescription desc = descriptions.FirstOrDefault();

                ConnectionString = desc.ConnectionString;

                var connectionStringMatch = connectionStringRegex.Match(ConnectionString);
                DefaultKey = connectionStringMatch.Groups[3].Value;
                DefaultIssuer = connectionStringMatch.Groups[2].Value;
            }
            else
            {
                DefaultKey = string.Empty;
                ConnectionString = string.Empty;
            }

        }

        public string Name { get; set; }

        public string Region { get; set; }

        public string DefaultIssuer { get; set; }
        public string DefaultKey { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string AcsManagementEndpoint { get; set; }

        public string ServiceBusEndpoint { get; set; }

        public string ConnectionString { get; set; }

        public override bool Equals(object obj)
        {
            ExtendedServiceBusNamespace lhs = obj as ExtendedServiceBusNamespace;

            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }

            return this.Name.Equals(lhs.Name) && this.Region.Equals(lhs.Region);
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return base.GetHashCode();
            }
            else
            {
                return this.Name.GetHashCode() ^ this.Region.GetHashCode();
            }
        }
    }
}
