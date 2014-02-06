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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.PSCmdlets
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands.CommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;
    using GetAzureHDInsightClusters.Extensions;
    using HDInsight.Logging;
    using ServiceLocation;

    /// <summary>
    ///     Cmdlet that creates a new HDInsight cluster.
    /// </summary>
    [Cmdlet(VerbsCommon.New, AzureHdInsightPowerShellConstants.AzureHDInsightCluster,
        DefaultParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
    [OutputType(typeof(AzureHDInsightCluster))]
    public class NewAzureHDInsightClusterCmdlet : AzureHDInsightCmdlet
    {
        private readonly INewAzureHDInsightClusterCommand command;

        /// <summary>
        ///     Initializes a new instance of the NewAzureHDInsightClusterCmdlet class.
        /// </summary>
        public NewAzureHDInsightClusterCmdlet()
        {
            this.command = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateCreate();
        }

        /// <inheritdoc />
        [Parameter(Position = 2, Mandatory = false, HelpMessage = "The management certificate used to manage the Azure subscription.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 3, Mandatory = false, HelpMessage = "The management certificate used to manage the Azure subscription.",
            ValueFromPipeline = false,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasCert)]
        public X509Certificate2 Certificate
        {
            get { return this.command.Certificate; }
            set { this.command.Certificate = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 8, Mandatory = false, HelpMessage = "The Endpoint to use when connecting to Azure.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 11, Mandatory = false, HelpMessage = "The Endpoint to use when connecting to Azure.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasCloudServiceName)]
        public string HostedService
        {
            get { return this.command.HostedService; }
            set { this.command.HostedService = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 9, Mandatory = true, HelpMessage = "The number of data nodes to use for the HDInsight cluster.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasNodes, AzureHdInsightPowerShellConstants.AliasSize)]
        public int ClusterSizeInNodes
        {
            get { return this.command.ClusterSizeInNodes; }
            set { this.command.ClusterSizeInNodes = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 1, Mandatory = true,
            HelpMessage = "The HDInsight cluster configuration to use when creating the new cluster (created by New-AzureHDInsightConfig).",
            ValueFromPipeline = true,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        public AzureHDInsightConfig Config
        {
            get
            {
                var result = new AzureHDInsightConfig();
                result.ClusterSizeInNodes = this.command.ClusterSizeInNodes;
                result.DefaultStorageAccount.StorageAccountName = this.command.DefaultStorageAccountName;
                result.DefaultStorageAccount.StorageAccountKey = this.command.DefaultStorageAccountKey;
                result.DefaultStorageAccount.StorageContainerName = this.command.DefaultStorageContainerName;
                result.AdditionalStorageAccounts.AddRange(this.command.AdditionalStorageAccounts);
                result.CoreConfiguration.AddRange(this.command.CoreConfiguration);
                result.HdfsConfiguration.AddRange(this.command.HdfsConfiguration);
                result.OozieConfiguration.ConfigurationCollection.AddRange(this.command.OozieConfiguration.ConfigurationCollection);
                result.HiveConfiguration.AdditionalLibraries = this.command.HiveConfiguration.AdditionalLibraries;
                result.HiveConfiguration.ConfigurationCollection.AddRange(this.command.HiveConfiguration.ConfigurationCollection);
                result.MapReduceConfiguration.ConfigurationCollection.AddRange(this.command.MapReduceConfiguration.ConfigurationCollection);
                result.MapReduceConfiguration.CapacitySchedulerConfigurationCollection.AddRange(
                    this.command.MapReduceConfiguration.CapacitySchedulerConfigurationCollection);
                return result;
            }

            set
            {
                if (value.IsNull())
                {
                    throw new ArgumentNullException("value", "The value for the configuration can not be null.");
                }
                this.command.ClusterSizeInNodes = value.ClusterSizeInNodes;
                this.command.DefaultStorageAccountName = value.DefaultStorageAccount.StorageAccountName;
                this.command.DefaultStorageAccountKey = value.DefaultStorageAccount.StorageAccountKey;
                this.command.DefaultStorageContainerName = value.DefaultStorageAccount.StorageContainerName;
                this.command.AdditionalStorageAccounts.AddRange(value.AdditionalStorageAccounts);
                this.command.CoreConfiguration.AddRange(value.CoreConfiguration);
                this.command.HdfsConfiguration.AddRange(value.HdfsConfiguration);
                this.command.MapReduceConfiguration.ConfigurationCollection.AddRange(value.MapReduceConfiguration.ConfigurationCollection);
                this.command.MapReduceConfiguration.CapacitySchedulerConfigurationCollection.AddRange(
                    value.MapReduceConfiguration.CapacitySchedulerConfigurationCollection);
                this.command.HiveConfiguration.AdditionalLibraries = value.HiveConfiguration.AdditionalLibraries;
                this.command.HiveConfiguration.ConfigurationCollection.AddRange(value.HiveConfiguration.ConfigurationCollection);
                this.command.OozieConfiguration.ConfigurationCollection.AddRange(value.OozieConfiguration.ConfigurationCollection);
                this.command.OozieConfiguration.AdditionalSharedLibraries = value.OozieConfiguration.AdditionalSharedLibraries;
                this.command.OozieConfiguration.AdditionalActionExecutorLibraries = value.OozieConfiguration.AdditionalActionExecutorLibraries;
                this.command.HiveMetastore = value.HiveMetastore;
                this.command.OozieMetastore = value.OozieMetastore;
            }
        }

        /// <inheritdoc />
        [Parameter(Position = 7, Mandatory = true, HelpMessage = "The user credentials for the HDInsight cluster.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 5, Mandatory = true, HelpMessage = "The user credentials for the HDInsight cluster.", ValueFromPipeline = false,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasCredentials)]
        public PSCredential Credential
        {
            get { return this.command.Credential; }
            set { this.command.Credential = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 5, Mandatory = true, HelpMessage = "The key to use for the default storage account.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasStorageKey)]
        public string DefaultStorageAccountKey
        {
            get { return this.command.DefaultStorageAccountKey; }
            set { this.command.DefaultStorageAccountKey = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 4, Mandatory = true, HelpMessage = "The default storage account to use for the new cluster.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasStorageAccount)]
        public string DefaultStorageAccountName
        {
            get { return this.command.DefaultStorageAccountName; }
            set { this.command.DefaultStorageAccountName = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 6, Mandatory = true, HelpMessage = "The container in the storage account to use for default HDInsight storage.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasStorageContainer)]
        public string DefaultStorageContainerName
        {
            get { return this.command.DefaultStorageContainerName; }
            set { this.command.DefaultStorageContainerName = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 7, Mandatory = false, HelpMessage = "The Endpoint to use when connecting to Azure.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 10, Mandatory = false, HelpMessage = "The Endpoint to use when connecting to Azure.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        public Uri EndPoint
        {
            get { return this.command.Endpoint; }
            set { this.command.Endpoint = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 3, Mandatory = true, HelpMessage = "The azure location where the new cluster should be created.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 4, Mandatory = true, HelpMessage = "The azure location where the new cluster should be created.",
            ValueFromPipeline = false,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasLoc)]
        public string Location
        {
            get { return this.command.Location; }
            set { this.command.Location = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "The name of the HDInsight cluster to locate.", ValueFromPipeline = true,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "The name of the HDInsight cluster to locate.", ValueFromPipeline = false,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasClusterName, AzureHdInsightPowerShellConstants.AliasDnsName)]
        public string Name
        {
            get { return this.command.Name; }
            set { this.command.Name = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 1, Mandatory = false, HelpMessage = "The subscription id for the Azure subscription.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 2, Mandatory = false, HelpMessage = "The subscription id for the Azure subscription.", ValueFromPipeline = false,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasSub)]
        public string Subscription
        {
            get { return this.command.Subscription; }
            set { this.command.Subscription = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 12, Mandatory = false, HelpMessage = "The version of the HDInsight cluster to create.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Position = 9, Mandatory = false, HelpMessage = "The version of the HDInsight cluster to create.", ValueFromPipeline = false,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByConfigWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasVersion)]
        public string Version
        {
            get { return this.command.Version; }
            set { this.command.Version = value; }
        }

        /// <inheritdoc />
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        /// <inheritdoc />
        protected override void EndProcessing()
        {
            DateTime start = DateTime.Now;
            string msg = string.Format(CultureInfo.CurrentCulture, "Create Cluster Started : {0}", start.ToString(CultureInfo.CurrentCulture));
            this.Logger.Log(Severity.Informational, Verbosity.Detailed, msg);
            try
            {
                this.command.Logger = this.Logger;
            this.command.CurrentSubscription = this.GetCurrentSubscription(this.Subscription, this.Certificate);
                Task task = this.command.EndProcessing();
                CancellationToken token = this.command.CancellationToken;
                while (!task.IsCompleted)
                {
                    this.WriteDebugLog();
                    msg = string.Format(CultureInfo.CurrentCulture, "Creating Cluster: {0}", this.Name);
                    this.WriteProgress(new ProgressRecord(0, msg, this.command.State.ToString()));
                    task.Wait(1000, token);
                }
                if (task.IsFaulted)
                {
                    throw new AggregateException(task.Exception);
                }
                foreach (AzureHDInsightCluster output in this.command.Output)
                {
                    this.WriteObject(output);
                }
                this.WriteDebugLog();
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                this.Logger.Log(Severity.Error, Verbosity.Normal, this.FormatException(ex));
                this.WriteDebugLog();
                if (type == typeof(AggregateException) || type == typeof(TargetInvocationException) || type == typeof(TaskCanceledException))
                {
                    ex.Rethrow();
                }
                else
                {
                    throw;
                }
            }
            msg = string.Format(CultureInfo.CurrentCulture, "Create Cluster Stopped : {0}", DateTime.Now.ToString(CultureInfo.CurrentCulture));
            this.Logger.Log(Severity.Informational, Verbosity.Detailed, msg);
            msg = string.Format(
                CultureInfo.CurrentCulture,
                "Create Cluster Executed for {0} minutes",
                (DateTime.Now - start).TotalMinutes.ToString(CultureInfo.CurrentCulture));
            this.Logger.Log(Severity.Informational, Verbosity.Detailed, msg);
            this.WriteDebugLog();
        }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        /// <inheritdoc />
        protected override void StopProcessing()
        {
            this.command.Cancel();
        }
    }
}
