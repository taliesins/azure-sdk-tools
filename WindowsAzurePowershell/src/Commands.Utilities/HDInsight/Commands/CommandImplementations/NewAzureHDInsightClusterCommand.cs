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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using CommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;
    using GetAzureHDInsightClusters.Extensions;

    internal class NewAzureHDInsightClusterCommand : AzureHDInsightClusterCommand<AzureHDInsightCluster>, INewAzureHDInsightClusterCommand
    {
        public NewAzureHDInsightClusterCommand()
        {
            this.AdditionalStorageAccounts = new List<AzureHDInsightStorageAccount>();
            this.CoreConfiguration = new ConfigValuesCollection();
            this.HdfsConfiguration = new ConfigValuesCollection();
            this.MapReduceConfiguration = new MapReduceConfiguration();
            this.HiveConfiguration = new HiveConfiguration();
            this.OozieConfiguration = new OozieConfiguration();
        }

        public ICollection<AzureHDInsightStorageAccount> AdditionalStorageAccounts { get; private set; }

        /// <inheritdoc />
        public int ClusterSizeInNodes { get; set; }

        /// <inheritdoc />
        public ConfigValuesCollection CoreConfiguration { get; set; }

        /// <inheritdoc />
        public PSCredential Credential { get; set; }

        /// <inheritdoc />
        public string DefaultStorageAccountKey { get; set; }

        /// <inheritdoc />
        public string DefaultStorageAccountName { get; set; }

        /// <inheritdoc />
        public string DefaultStorageContainerName { get; set; }

        /// <inheritdoc />
        public ConfigValuesCollection HdfsConfiguration { get; set; }

        /// <inheritdoc />
        public HiveConfiguration HiveConfiguration { get; set; }

        public AzureHDInsightMetastore HiveMetastore { get; set; }

        /// <inheritdoc />
        public string Location { get; set; }

        /// <inheritdoc />
        public MapReduceConfiguration MapReduceConfiguration { get; set; }

        /// <inheritdoc />
        public OozieConfiguration OozieConfiguration { get; set; }

        public AzureHDInsightMetastore OozieMetastore { get; set; }

        public ClusterState State { get; private set; }

        /// <inheritdoc />
        public string Version { get; set; }

        public override async Task EndProcessing()
        {
            IHDInsightClient client = this.GetClient();
            client.ClusterProvisioning += this.ClientOnClusterProvisioning;
            ClusterCreateParameters createClusterRequest = this.GetClusterCreateParameters();
            var cluster = await client.CreateClusterAsync(createClusterRequest);
            this.Output.Add(new AzureHDInsightCluster(cluster));
        }

        internal ClusterCreateParameters GetClusterCreateParameters()
        {
            var createClusterRequest = new ClusterCreateParameters();
            createClusterRequest.Name = this.Name;
            createClusterRequest.Version = this.Version;
            createClusterRequest.Location = this.Location;

            createClusterRequest.CoreConfiguration.AddRange(this.CoreConfiguration);
            createClusterRequest.HdfsConfiguration.AddRange(this.HdfsConfiguration);
            createClusterRequest.MapReduceConfiguration.ConfigurationCollection.AddRange(this.MapReduceConfiguration.ConfigurationCollection);
            createClusterRequest.MapReduceConfiguration.CapacitySchedulerConfigurationCollection.AddRange(
                this.MapReduceConfiguration.CapacitySchedulerConfigurationCollection);
            createClusterRequest.HiveConfiguration.AdditionalLibraries = this.HiveConfiguration.AdditionalLibraries;
            createClusterRequest.HiveConfiguration.ConfigurationCollection.AddRange(this.HiveConfiguration.ConfigurationCollection);
            createClusterRequest.OozieConfiguration.ConfigurationCollection.AddRange(this.OozieConfiguration.ConfigurationCollection);
            createClusterRequest.OozieConfiguration.AdditionalSharedLibraries = this.OozieConfiguration.AdditionalSharedLibraries;
            createClusterRequest.OozieConfiguration.AdditionalActionExecutorLibraries = this.OozieConfiguration.AdditionalActionExecutorLibraries;

            createClusterRequest.DefaultStorageAccountName = this.DefaultStorageAccountName;
            createClusterRequest.DefaultStorageAccountKey = this.DefaultStorageAccountKey;
            createClusterRequest.DefaultStorageContainer = this.DefaultStorageContainerName;
            createClusterRequest.UserName = this.Credential.UserName;
            createClusterRequest.Password = this.Credential.GetCleartextPassword();
            createClusterRequest.ClusterSizeInNodes = this.ClusterSizeInNodes;
            createClusterRequest.AdditionalStorageAccounts.AddRange(
                this.AdditionalStorageAccounts.Select(act => new WabStorageAccountConfiguration(act.StorageAccountName, act.StorageAccountKey)));
            if (this.HiveMetastore.IsNotNull())
            {
                createClusterRequest.HiveMetastore = new Metastore(
                    this.HiveMetastore.SqlAzureServerName,
                    this.HiveMetastore.DatabaseName,
                    this.HiveMetastore.Credential.UserName,
                    this.HiveMetastore.Credential.GetCleartextPassword());
            }
            if (this.OozieMetastore.IsNotNull())
            {
                createClusterRequest.OozieMetastore = new Metastore(
                    this.OozieMetastore.SqlAzureServerName,
                    this.OozieMetastore.DatabaseName,
                    this.OozieMetastore.Credential.UserName,
                    this.OozieMetastore.Credential.GetCleartextPassword());
            }
            return createClusterRequest;
        }

        private void ClientOnClusterProvisioning(object sender, ClusterProvisioningStatusEventArgs clusterProvisioningStatusEventArgs)
        {
            this.State = clusterProvisioningStatusEventArgs.State;
        }
    }
}
