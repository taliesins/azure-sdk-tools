﻿﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Sql;
    using Microsoft.WindowsAzure.Management.Sql.Models;

    /// <summary>
    /// Implementation of the <see cref="IServerDataServiceContext"/> with Certificate authentication.
    /// </summary>
    public partial class ServerDataServiceCertAuth : IServerDataServiceContext
    {
        #region Private Fields

        /// <summary>
        /// The number of bytes in 1 gigabyte.
        /// </summary>
        private const long BytesIn1Gb = 1 * 1024L * 1024L * 1024L;

        /// <summary>
        /// The previous request's client request ID
        /// </summary>
        private string clientRequestId;

        /// <summary>
        /// The name of the server we are connected to.
        /// </summary>
        private readonly string serverName;

        /// <summary>
        /// The subscription used to connect and authenticate.
        /// </summary>
        private readonly WindowsAzureSubscription subscription;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataServicesCertAuth"/> class
        /// </summary>
        /// <param name="subscription">The subscription used to connect and authenticate.</param>
        /// <param name="serverName">The name of the server to connect to.</param>
        private ServerDataServiceCertAuth(
            WindowsAzureSubscription subscription,
            string serverName)
        {
            this.serverName = serverName;
            this.subscription = subscription;
        }

        #region Public Properties

        /// <summary>
        /// Gets the client per-session tracing ID.
        /// </summary>
        public string ClientSessionId
        {
            get
            {
                return SqlDatabaseCmdletBase.clientSessionId;
            }
        }

        /// <summary>
        /// Gets the previous request's client request ID.
        /// </summary>
        public string ClientRequestId
        {
            get
            {
                return this.clientRequestId;
            }
        }

        /// <summary>
        /// Gets the name of the server for this context.
        /// </summary>
        public string ServerName
        {
            get
            {
                return this.serverName;
            }
        }

        #endregion

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceCertAuth"/> class
        /// which connects to the specified server using the specified subscription credentials.
        /// </summary>
        /// <param name="subscription">The subscription used to connect and authenticate.</param>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <returns>An instance of <see cref="ServerDataServiceCertAuth"/> class.</returns>
        public static ServerDataServiceCertAuth Create(
            string serverName,
            WindowsAzureSubscription subscription)
        {
            if (string.IsNullOrEmpty(serverName))
            {
                throw new ArgumentException("serverName");
            }

            SqlDatabaseCmdletBase.ValidateSubscription(subscription);

            // Create a new ServerDataServiceCertAuth object to be used
            return new ServerDataServiceCertAuth(
                subscription,
                serverName);
        }

        #region IServerDataServiceContext Members

        /// <summary>
        /// Ensures any extra property on the given <paramref name="obj"/> is loaded.
        /// </summary>
        /// <param name="obj">The object that needs the extra properties.</param>
        public void LoadExtraProperties(object obj)
        {
            try
            {
                Database database = obj as Database;
                if (database != null)
                {
                    this.LoadExtraProperties(database);
                    return;
                }
            }
            catch
            {
                // Ignore exceptions when loading extra properties, for backward compatibility.
            }
        }

        #region Database Operations

        /// <summary>
        /// Gets a list of all the databases in the current context.
        /// </summary>
        /// <returns>An array of databases in the current context</returns>
        public Database[] GetDatabases()
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the list of databases
            DatabaseListResponse response = sqlManagementClient.Databases.List(this.serverName);

            // Construct the resulting Database objects
            Database[] databases = response.Databases.Select((db) => CreateDatabaseFromResponse(db)).ToArray();
            return databases;
        }

        /// <summary>
        /// Retrieve a specific database from the current context
        /// </summary>
        /// <param name="databaseName">The name of the database to retrieve</param>
        /// <returns>A database object</returns>
        public Database GetDatabase(string databaseName)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified database
            DatabaseGetResponse response = sqlManagementClient.Databases.Get(
                this.serverName,
                databaseName);

            // Construct the resulting Database object
            Database database = CreateDatabaseFromResponse(response);
            return database;
        }

        /// <summary>
        /// Creates a new sql database.
        /// </summary>
        /// <param name="databaseName">The name for the new database</param>
        /// <param name="databaseMaxSizeInGB">The maximum size of the new database</param>
        /// <param name="databaseCollation">The collation for the new database</param>
        /// <param name="databaseEdition">The edition for the new database</param>
        /// <returns>The newly created Sql Database</returns>
        public Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSizeInGB,
            string databaseCollation,
            DatabaseEdition databaseEdition,
            ServiceObjective serviceObjective)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Create the database
            DatabaseCreateResponse response = sqlManagementClient.Databases.Create(
                this.serverName,
                new DatabaseCreateParameters()
                {
                    Name = databaseName,
                    Edition = databaseEdition != DatabaseEdition.None ?
                        databaseEdition.ToString() : DatabaseEdition.Web.ToString(),
                    CollationName = databaseCollation ?? string.Empty,
                    MaximumDatabaseSizeInGB = databaseMaxSizeInGB ??
                        (databaseEdition == DatabaseEdition.Business || databaseEdition == DatabaseEdition.Premium ? 10 : 1),
                    ServiceObjectiveId = serviceObjective != null ? serviceObjective.Id.ToString() : null,
                });

            // Construct the resulting Database object
            Database database = CreateDatabaseFromResponse(response);
            return database;
        }

        /// <summary>
        /// Update a database on the server.
        /// </summary>
        /// <param name="databaseName">The name of the database to modify.</param>
        /// <param name="newDatabaseName">The new name of the database.</param>
        /// <param name="databaseMaxSizeInGB">The new maximum size of the database.</param>
        /// <param name="databaseEdition">The new edition of the database.</param>
        /// <param name="serviceObjective">The new service objective of the database.</param>
        /// <returns>The updated database.</returns>
        public Database UpdateDatabase(
            string databaseName,
            string newDatabaseName,
            int? databaseMaxSizeInGB,
            DatabaseEdition? databaseEdition,
            ServiceObjective serviceObjective)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified database
            DatabaseGetResponse database = sqlManagementClient.Databases.Get(
                this.serverName,
                databaseName);

            // Update the database with the new properties
            DatabaseUpdateResponse response = sqlManagementClient.Databases.Update(
                this.serverName,
                databaseName,
                new DatabaseUpdateParameters()
                {
                    Id = database.Id,
                    Name = !string.IsNullOrEmpty(newDatabaseName) ?
                        newDatabaseName : database.Name,
                    Edition = databaseEdition.HasValue && (databaseEdition != DatabaseEdition.None) ?
                        databaseEdition.ToString() : (database.Edition ?? string.Empty),
                    CollationName = database.CollationName ?? string.Empty,
                    MaximumDatabaseSizeInGB = databaseMaxSizeInGB.HasValue ?
                        databaseMaxSizeInGB.Value : database.MaximumDatabaseSizeInGB,
                    ServiceObjectiveId = serviceObjective != null ?
                        serviceObjective.Id.ToString() : null,
                });

            // Construct the resulting Database object
            Database updatedDatabase = CreateDatabaseFromResponse(response);
            return updatedDatabase;
        }

        /// <summary>
        /// Remove a database from a server
        /// </summary>
        /// <param name="databaseName">The name of the database to delete</param>
        public void RemoveDatabase(string databaseName)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the list of databases
            OperationResponse response = sqlManagementClient.Databases.Delete(
                this.serverName,
                databaseName);
        }

        #endregion

        #region Service Objective Operations

        /// <summary>
        /// Retrieves the list of all service objectives on the server.
        /// </summary>
        /// <returns>An array of all service objectives on the server.</returns>
        public ServiceObjective[] GetServiceObjectives()
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified database
            ServiceObjectiveListResponse response = sqlManagementClient.ServiceObjectives.List(
                this.serverName);

            // Construct the resulting Database object
            ServiceObjective[] serviceObjectives = response.Select(serviceObjective => CreateServiceObjectiveFromResponse(serviceObjective)).ToArray();
            return serviceObjectives;
        }

        /// <summary>
        /// Retrieve information on service objective with the specified name
        /// </summary>
        /// <param name="serviceObjectiveName">The service objective to retrieve.</param>
        /// <returns>
        /// An object containing the information about the specific service objective.
        /// </returns>
        public ServiceObjective GetServiceObjective(string serviceObjectiveName)
        {
            ServiceObjective serviceObjective = GetServiceObjectives()
                .Where(s => s.Name == serviceObjectiveName)
                .FirstOrDefault();
            if (serviceObjective == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.ServiceObjectiveNotFound,
                        this.ServerName,
                        serviceObjectiveName));
            }
            return serviceObjective;
        }

        /// <summary>
        /// Retrieve information on latest service objective with service objective
        /// </summary>
        /// <param name="serviceObjective">The service objective to refresh.</param>
        /// <returns>
        /// An object containing the information about the specific service objective.
        /// </returns>
        public ServiceObjective GetServiceObjective(ServiceObjective serviceObjectiveToRefresh)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified database
            ServiceObjectiveGetResponse response = sqlManagementClient.ServiceObjectives.Get(
                this.serverName,
                serviceObjectiveToRefresh.Id.ToString());

            // Construct the resulting Database object
            ServiceObjective serviceObjective = CreateServiceObjectiveFromResponse(response);
            return serviceObjective;
        }

        /// <summary>
        /// Get a specific quota for a server
        /// </summary>
        /// <param name="quotaName">The name of the quota to retrieve</param>
        /// <returns>A quota object.</returns>
        public ServerQuota GetQuota(string quotaName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get a list of all quotas for a server
        /// </summary>
        /// <returns>An array of server quota objects</returns>
        public ServerQuota[] GetQuotas()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Database Operation Functions

        /// <summary>
        /// Retrieve information on operation with the guid 
        /// </summary>
        /// <param name="OperationGuid">The Guid of the operation to retrieve.</param>
        /// <returns>An object containing the information about the specific operation.</returns>
        public DatabaseOperation GetDatabaseOperation(Guid OperationGuid)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the specified Operation
            DatabaseOperationGetResponse response = sqlManagementClient.DatabaseOperations.Get(
                this.serverName,
                OperationGuid.ToString());

            // Construct the resulting Operation object
            DatabaseOperation operation = CreateDatabaseOperationFromResponse(response);
            return operation;
        }

        /// <summary>
        /// Retrieves the list of all operations on the database.
        /// </summary>
        /// <param name="databaseName">The name of database to retrieve operations.</param>
        /// <returns>An array of all operations on the database.</returns>
        public DatabaseOperation[] GetDatabaseOperations(string databaseName)
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve all operations on specified database
            DatabaseOperationListResponse response = sqlManagementClient.DatabaseOperations.ListByDatabase(
                this.serverName,
                databaseName);

            // For any database which has ever been created, there should be at least one operation
            if (response.Count() == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.DatabaseOperationNotFoundOnDatabase,
                        this.ServerName,
                        databaseName));
            }

            // Construct the resulting database operations
            DatabaseOperation[] operations = response.Select(operation => CreateDatabaseOperationsFromResponse(operation)).ToArray();
            return operations;
        }

        /// <summary>
        /// Retrieves the list of all databases' operations on the server.
        /// </summary>
        /// <returns>An array of all operations on the server.</returns>
        public DatabaseOperation[] GetDatabasesOperations()
        {
            this.clientRequestId = SqlDatabaseCmdletBase.GenerateClientTracingId();

            // Get the SQL management client
            SqlManagementClient sqlManagementClient = this.subscription.CreateClient<SqlManagementClient>();
            this.AddTracingHeaders(sqlManagementClient);

            // Retrieve the operations on specified server 
            // We do not validate the number of operations returned since it's possible that there is no 
            // database operations on a new created server.
            DatabaseOperationListResponse response = sqlManagementClient.DatabaseOperations.ListByServer(
                this.serverName);

            // Construct the resulting database operations array
            DatabaseOperation[] operations = response.Select(operation => CreateDatabaseOperationsFromResponse(operation)).ToArray();
            return operations;
        }

        #endregion

        #endregion

        #region Helper functions

        private DimensionSetting CreateDimensionSettings(string name, string id, string description, byte ordinal, bool isDefault)
        {
            return new DimensionSetting()
            {
                Name = name,
                Id = Guid.Parse(id),
                Description = description,
                Ordinal = ordinal,
                IsDefault = isDefault
            };
        }

        private Collection<DimensionSetting> CreateDimensionSettingsFromResponse(IList<ServiceObjectiveListResponse.ServiceObjective.DimensionSettingResponse> dimensionSettingsList)
        {
            Collection<DimensionSetting> result = new Collection<DimensionSetting>();
            foreach (var response in dimensionSettingsList)
            {
                result.Add(CreateDimensionSettings(
                    response.Name,
                    response.Id,
                    response.Description,
                    response.Ordinal,
                    response.IsDefault
                    ));
            }
            return result;
        }

        private Collection<DimensionSetting> CreateDimensionSettingsFromResponse(IList<ServiceObjectiveGetResponse.DimensionSettingResponse> dimensionSettingsList)
        {
            Collection<DimensionSetting> result = new Collection<DimensionSetting>();
            foreach (var response in dimensionSettingsList)
            {
                result.Add(CreateDimensionSettings(
                    response.Name,
                    response.Id,
                    response.Description,
                    response.Ordinal,
                    response.IsDefault
                    ));
            }
            return result;
        }

        private ServiceObjective CreateServiceObjectiveFromResponse(ServiceObjectiveListResponse.ServiceObjective response)
        {
            return new ServiceObjective()
            {
                Name = response.Name,
                Id = Guid.Parse(response.Id),
                IsDefault = response.IsDefault,
                IsSystem = response.IsSystem,
                Enabled = response.Enabled,
                Description = response.Description,
                Context = this,
                DimensionSettings = CreateDimensionSettingsFromResponse(response.DimensionSettings)
            };
        }

        private ServiceObjective CreateServiceObjectiveFromResponse(ServiceObjectiveGetResponse response)
        {
            return new ServiceObjective()
            {
                Name = response.Name,
                Id = Guid.Parse(response.Id),
                IsDefault = response.IsDefault,
                IsSystem = response.IsSystem,
                Enabled = response.Enabled,
                Description = response.Description,
                Context = this,
                DimensionSettings = CreateDimensionSettingsFromResponse(response.DimensionSettings)
            };
        }

        private DatabaseOperation CreateDatabaseOperation(string name, string state, string id, int stateId, string sessionActivityId, string databaseName, int percentComplete, int errorCode, string error, int errorSeverity, int errorState, DateTime startTime, DateTime lastModifyTime)
        {
            return new DatabaseOperation()
            {
                Name = name,
                State = state,
                Id = Guid.Parse(id),
                StateId = stateId,
                SessionActivityId = Guid.Parse(sessionActivityId),
                DatabaseName = databaseName,
                PercentComplete = percentComplete,
                ErrorCode = errorCode,
                Error = error,
                ErrorSeverity = errorSeverity,
                ErrorState = errorState,
                StartTime = startTime,
                LastModifyTime = lastModifyTime,
            };
        }

        private DatabaseOperation CreateDatabaseOperationFromResponse(DatabaseOperationGetResponse response)
        {
            return CreateDatabaseOperation(
                    response.Name,
                    response.State,
                    response.Id,
                    response.StateId,
                    response.SessionActivityId,
                    response.DatabaseName,
                    response.PercentComplete,
                    response.ErrorCode,
                    response.Error,
                    response.ErrorSeverity,
                    response.ErrorState,
                    response.StartTime,
                    response.LastModifyTime
                    );
        }

        private DatabaseOperation CreateDatabaseOperationsFromResponse(DatabaseOperationListResponse.DatabaseOperation response)
        {
            return CreateDatabaseOperation(
                    response.Name,
                    response.State,
                    response.Id,
                    response.StateId,
                    response.SessionActivityId,
                    response.DatabaseName,
                    response.PercentComplete,
                    response.ErrorCode,
                    response.Error,
                    response.ErrorSeverity,
                    response.ErrorState,
                    response.StartTime,
                    response.LastModifyTime
                    );
        }

        /// <summary>
        /// Given a <see cref="DatabaseGetResponse"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseGetResponse response)
        {
            return this.CreateDatabaseFromResponse(
                response.Id,
                response.Name,
                response.CreationDate,
                response.Edition,
                response.CollationName,
                response.MaximumDatabaseSizeInGB,
                response.IsFederationRoot,
                response.IsSystemObject,
                response.SizeMB,
                response.ServiceObjectiveAssignmentErrorCode,
                response.ServiceObjectiveAssignmentErrorDescription,
                response.ServiceObjectiveAssignmentState,
                response.ServiceObjectiveAssignmentStateDescription,
                response.ServiceObjectiveAssignmentSuccessDate,
                response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a <see cref="DatabaseListResponse.Database"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseListResponse.Database response)
        {
            return this.CreateDatabaseFromResponse(
                response.Id,
                response.Name,
                response.CreationDate,
                response.Edition,
                response.CollationName,
                response.MaximumDatabaseSizeInGB,
                response.IsFederationRoot,
                response.IsSystemObject,
                response.SizeMB,
                response.ServiceObjectiveAssignmentErrorCode,
                response.ServiceObjectiveAssignmentErrorDescription,
                response.ServiceObjectiveAssignmentState,
                response.ServiceObjectiveAssignmentStateDescription,
                response.ServiceObjectiveAssignmentSuccessDate,
                response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a <see cref="DatabaseCreateResponse"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseCreateResponse response)
        {
            return this.CreateDatabaseFromResponse(
               response.Id,
               response.Name,
               response.CreationDate,
               response.Edition,
               response.CollationName,
               response.MaximumDatabaseSizeInGB,
               response.IsFederationRoot,
               response.IsSystemObject,
               response.SizeMB,
               response.ServiceObjectiveAssignmentErrorCode,
               response.ServiceObjectiveAssignmentErrorDescription,
               response.ServiceObjectiveAssignmentState,
               response.ServiceObjectiveAssignmentStateDescription,
               response.ServiceObjectiveAssignmentSuccessDate,
               response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a <see cref="DatabaseUpdateResponse"/> this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="response">The response to turn into a <see cref="Database"/></param>
        /// <returns>a <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(DatabaseUpdateResponse response)
        {
            return this.CreateDatabaseFromResponse(
                response.Id,
                response.Name,
                response.CreationDate,
                response.Edition,
                response.CollationName,
                response.MaximumDatabaseSizeInGB,
                response.IsFederationRoot,
                response.IsSystemObject,
                response.SizeMB,
                response.ServiceObjectiveAssignmentErrorCode,
                response.ServiceObjectiveAssignmentErrorDescription,
                response.ServiceObjectiveAssignmentState,
                response.ServiceObjectiveAssignmentStateDescription,
                response.ServiceObjectiveAssignmentSuccessDate,
                response.ServiceObjectiveId);
        }

        /// <summary>
        /// Given a set of database properties this will create and return a <see cref="Database"/> 
        /// object with the fields filled in.
        /// </summary>
        /// <param name="id">The database Id.</param>
        /// <param name="name">The database name.</param>
        /// <param name="creationDate">The database creation date.</param>
        /// <param name="edition">The database edition.</param>
        /// <param name="collationName">The database collation name.</param>
        /// <param name="maximumDatabaseSizeInGB">The database maximum size.</param>
        /// <param name="isFederationRoot">Whether or not the database is a federation root.</param>
        /// <param name="isSystemObject">Whether or not the database is a system object.</param>
        /// <param name="sizeMB">The current database size.</param>
        /// <param name="serviceObjectiveAssignmentErrorCode">
        /// The last error code received for service objective assignment change.
        /// </param>
        /// <param name="serviceObjectiveAssignmentErrorDescription">
        /// The last error received for service objective assignment change.
        /// </param>
        /// <param name="serviceObjectiveAssignmentState">
        /// The state of the current service objective assignment.
        /// </param>
        /// <param name="serviceObjectiveAssignmentStateDescription">
        /// The state description for the current service objective assignment.
        /// </param>
        /// <param name="serviceObjectiveAssignmentSuccessDate">
        /// The last success date for a service objective assignment on this database.
        /// </param>
        /// <param name="serviceObjectiveId">The service objective Id for this database.</param>
        /// <returns>A <see cref="Database"/> object.</returns>
        private Database CreateDatabaseFromResponse(
            int id,
            string name,
            DateTime creationDate,
            string edition,
            string collationName,
            long maximumDatabaseSizeInGB,
            bool isFederationRoot,
            bool isSystemObject,
            string sizeMB,
            string serviceObjectiveAssignmentErrorCode,
            string serviceObjectiveAssignmentErrorDescription,
            string serviceObjectiveAssignmentState,
            string serviceObjectiveAssignmentStateDescription,
            string serviceObjectiveAssignmentSuccessDate,
            string serviceObjectiveId)
        {
            Database result = new Database()
            {
                Id = id,
                Name = name,
                CollationName = collationName,
                CreationDate = creationDate,
                Edition = edition,
                MaxSizeGB = (int)maximumDatabaseSizeInGB,
                MaxSizeBytes = maximumDatabaseSizeInGB * BytesIn1Gb,
                IsFederationRoot = isFederationRoot,
                IsSystemObject = isSystemObject,
            };

            // Parse any additional database information
            if (!string.IsNullOrEmpty(sizeMB))
            {
                result.SizeMB = decimal.Parse(sizeMB, CultureInfo.InvariantCulture);
            }

            // Parse the service objective information
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentErrorCode))
            {
                result.ServiceObjectiveAssignmentErrorCode = int.Parse(serviceObjectiveAssignmentErrorCode);
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentErrorDescription))
            {
                result.ServiceObjectiveAssignmentErrorDescription = serviceObjectiveAssignmentErrorDescription;
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentState))
            {
                result.ServiceObjectiveAssignmentState = byte.Parse(serviceObjectiveAssignmentState);
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentStateDescription))
            {
                result.ServiceObjectiveAssignmentStateDescription = serviceObjectiveAssignmentStateDescription;
            }
            if (!string.IsNullOrEmpty(serviceObjectiveAssignmentSuccessDate))
            {
                result.ServiceObjectiveAssignmentSuccessDate = DateTime.Parse(serviceObjectiveAssignmentSuccessDate, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(serviceObjectiveId))
            {
                result.ServiceObjectiveId = Guid.Parse(serviceObjectiveId);
            }

            this.LoadExtraProperties(result);

            return result;
        }

        #endregion

        /// <summary>
        /// Add the tracing session and request headers to the client.
        /// </summary>
        /// <param name="sqlManagementClient">The client to add the headers on.</param>
        private void AddTracingHeaders(SqlManagementClient sqlManagementClient)
        {
            sqlManagementClient.HttpClient.DefaultRequestHeaders.Add(
                Constants.ClientSessionIdHeaderName,
                this.ClientSessionId);
            sqlManagementClient.HttpClient.DefaultRequestHeaders.Add(
                Constants.ClientRequestIdHeaderName,
                this.ClientRequestId);
        }

        /// <summary>
        /// Ensures any extra property on the given <paramref name="database"/> is loaded.
        /// </summary>
        /// <param name="database">The database that needs the extra properties.</param>
        private void LoadExtraProperties(Database database)
        {
            // Fill in the context property
            database.Context = this;
        }
    }
}
