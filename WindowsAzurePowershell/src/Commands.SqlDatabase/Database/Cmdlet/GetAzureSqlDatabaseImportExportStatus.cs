﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.ImportExport;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Sql;
    using Microsoft.WindowsAzure.Management.Sql.Models;

    /// <summary>
    /// Exports a database from SQL Azure into blob storage.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseImportExportStatus", ConfirmImpact = ConfirmImpact.None)]
    public class GetAzureSqlDatabaseImportExportStatus : SqlDatabaseCmdletBase
    {
        #region Parameter sets

        /// <summary>
        /// The name of the parameter set that uses a RequestObject
        /// </summary>
        internal const string ByRequestObjectParameterSet =
            "ByRequestObject";

        /// <summary>
        /// The name of the parameter set that gets the connection information from
        /// the parameters
        /// </summary>
        internal const string ByConnectionInfoParameterSet =
            "ByConnectionInfo";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the user name for connecting to the database
        /// </summary>
        [Parameter(Mandatory = true, Position = 0,
            ParameterSetName = ByConnectionInfoParameterSet,
            HelpMessage = "The user name for connecting to the database")]
        [ValidateNotNullOrEmpty]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for connecting to the database
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            ParameterSetName = ByConnectionInfoParameterSet,
            HelpMessage = "The password for connecting to the database")]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the server the database resides in
        /// </summary>
        [Parameter(Mandatory = true, Position = 2,
            ParameterSetName = ByConnectionInfoParameterSet,
            HelpMessage = "The name of the server the database is in")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the request Id of the operation to get the status of
        /// </summary>
        [Parameter(Mandatory = true, Position = 3,
            ParameterSetName = ByConnectionInfoParameterSet,
            HelpMessage = "The request Id of the operation to get the status of")]
        [ValidateNotNullOrEmpty]
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the request object 
        /// </summary>
        [Parameter(Mandatory = true, Position = 0,
            ParameterSetName = ByRequestObjectParameterSet,
            HelpMessage = "The ImportExportRequest object returned from starting the request")]
        [ValidateNotNullOrEmpty]
        public ImportExportRequest Request { get; set; }

        #endregion

        /// <summary>
        /// Performs the call to export database using the server data service context channel.
        /// </summary>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <param name="fullyQualifiedServerName">The fully qualfied name of the server to connect to.</param>
        /// <param name="userName">The username for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <param name="requestId">The request Id of the operation to query</param>
        /// <returns>The status of the import/export operation</returns>
        internal IEnumerable<StatusInfo> GetAzureSqlDatabaseImportExportStatusProcess(
            string serverName,
            string fullyQualifiedServerName,
            string userName,
            string password,
            string requestId)
        {
            // Get the SQL management client for the current subscription
            SqlManagementClient sqlManagementClient = GetCurrentSqlClient();

            // Start the database export operation
            DacGetStatusResponse response = sqlManagementClient.Dacs.GetStatus(
                serverName,
                fullyQualifiedServerName,
                userName,
                password,
                requestId);

            // Construct the result
            IEnumerable<StatusInfo> result = response.StatusInfoList.Select(status => new StatusInfo
            {
                BlobUri = status.BlobUri.ToString(),
                ServerName = status.ServerName,
                DatabaseName = status.DatabaseName,
                Status = status.Status,
                RequestId = status.RequestId,
                LastModifiedTime = status.LastModifiedTime,
                QueuedTime = status.QueuedTime,
                RequestType = status.RequestType,
                ErrorMessage = status.ErrorMessage,
            });

            return result;
        }

        /// <summary>
        /// Process the export request
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                // Parse the required information given the parameter set
                string serverName = null;
                string userName = null;
                string password = null;
                string requestId = null;
                switch (this.ParameterSetName)
                {
                    case ByRequestObjectParameterSet:
                        serverName = this.Request.ServerName;
                        userName = this.Request.SqlCredentials.UserName;
                        password = this.Request.SqlCredentials.Password;
                        requestId = this.Request.RequestGuid;
                        break;

                    case ByConnectionInfoParameterSet:
                        serverName = this.ServerName;
                        userName = this.Username;
                        password = this.Password;
                        requestId = this.RequestId;
                        break;

                    default:
                        throw new NotSupportedException("ParameterSet");
                }

                var status = this.GetAzureSqlDatabaseImportExportStatusProcess(
                    serverName,
                    serverName + DataServiceConstants.AzureSqlDatabaseDnsSuffix,
                    userName,
                    password,
                    requestId);

                this.WriteObject(status);
            }
            catch (Exception ex)
            {
                this.WriteErrorDetails(ex);
            }
        }
    }
}
