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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudService.AzureTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using Common;
    using Common.XmlSchema.ServiceDefinitionSchema;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;

    /// <summary>
    /// Package services for deployment to Azure.
    /// </summary>
    public class CsPack : AzureTool
    {
        /// <summary>
        /// Create a .cspkg package by calling the CsPack.exe tool.
        /// </summary>
        /// <param name="definition">Service definition</param>
        /// <param name="rootPath">Path to the service definition</param>
        /// <param name="type">Deployment type</param>
        /// <param name="standardOutput">Standard output</param>
        /// <param name="standardError">Standard error</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public void CreatePackage(ServiceDefinition definition, CloudProjectPathInfo paths, DevEnv type, out string standardOutput, out string standardError)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    "definition",
                    string.Format(Resources.InvalidOrEmptyArgumentMessage, "Service definition"));
            }
            if (string.IsNullOrEmpty(paths.RootPath))
            {
                throw new ArgumentException(Resources.InvalidRootNameMessage, "rootPath");
            }

            // Track the directories that are created by GetOrCreateCleanPath
            // to avoid publishing iisnode log files so we can delete the temp
            // copies when we're finished packaging
            Dictionary<string, string> tempDirectories = new Dictionary<string, string>();
            try
            {
                string roles =
                    // Get the names of all web and worker roles
                    Enumerable.Concat(
                        definition.WebRole.NonNull().Select(role => role.name),
                        definition.WorkerRole.NonNull().Select(role => role.name))
                    // Get the name and safe path for each role (i.e., if the
                    // role has files that shouldn't be packaged, it'll be
                    // copied to a temp location without those files)
                    .Select(name => GetOrCreateCleanPath(paths.RolesPath, name, tempDirectories, type))
                    // Format the role name and path as a role argument
                    .Select(nameAndPath => string.Format(Resources.RoleArgTemplate, nameAndPath.Key, nameAndPath.Value))
                    // Join all the role arguments together into one
                    .DefaultIfEmpty(string.Empty)
                    .Aggregate(string.Concat);

                string sites =
                    // Get all of the web roles
                    definition.WebRole.NonNull()
                    // Get all the sites in each role and format them all as
                    // site arguments
                    .SelectMany(role =>
                        // Format each site as a site argument
                        role.Sites.Site.Select(site =>
                            string.Format(
                                Resources.SitesArgTemplate,
                                role.name,
                                site.name,
                                tempDirectories.GetValueOrDefault(role.name, paths.RolesPath))))
                    // Join all the site arguments together into one
                    .DefaultIfEmpty(string.Empty)
                    .Aggregate(string.Concat);

                string args = string.Format(
                    type == DevEnv.Local ? Resources.CsPackLocalArg : Resources.CsPackCloudArg,
                    paths.RootPath,
                    roles,
                    sites);

                // Run CsPack to generate the package
                ProcessHelper.StartAndWaitForProcess(
                    new ProcessStartInfo(
                        Path.Combine(AzureSdkBinDirectory, Resources.CsPackExe),
                        args),
                    out standardOutput,
                    out standardError);
            }
            finally
            {
                // Cleanup any temp directories
                tempDirectories.Values.ForEach(dir => Directory.Delete(dir, true));
            }
        }

        /// <summary>
        /// Get or create a path to the role.  This is used to sanitize the
        /// contents of a role before it's packaged if it contains files that
        /// shouldn't be packaged.  We copy the contents to a temp directory,
        /// delete the offending files, and use the temp directory for
        /// packaging.  The temp directories are collected in the
        /// tempDirectories list so they can be cleaned up when packaging is
        /// complete.
        /// </summary>
        /// <remarks>
        /// This is a temporary workaround to prevent node logging information
        /// from being packaged and deployed with production applications. 
        /// This method should be removed when we have a proper fix to bug
        /// https://github.com/WindowsAzure/azure-sdk-tools/issues/111
        /// </remarks>
        /// <param name="root">The root path.</param>
        /// <param name="name">Name of the role.</param>
        /// <param name="tempDirectories">
        /// A collection of temporary directories that have been created to
        /// remove files that should not be published.  This will be added to
        /// by GetOrCreateCleanPath if a new directory is created that should
        /// be cleaned up once the package has been generated.
        /// </param>
        /// <returns>
        /// A pair containing the path to the role and the name of the role.
        /// </returns>
        private static KeyValuePair<string, string> GetOrCreateCleanPath(string root, string name, Dictionary<string, string> tempDirectories, DevEnv type)
        {
            string path = Path.Combine(root, name);

            // Check if the role has any "*.logs" directories that iisnode may
            // have left during emulation
            if (type == DevEnv.Local || GetLogDirectories(path).Length == 0)
            {
                return new KeyValuePair<string, string>(name, root);
            }
            else
            {
                // Create a temporary directory
                string tempPath = General.CreateTempDirectory();
                tempDirectories[name] = tempPath;
                
                // Copy the role's directory to the temp directory
                string newPath = Path.Combine(tempPath, name);
                General.CopyDirectory(Path.Combine(root, name), newPath);

                // Remove the offending files
                GetLogDirectories(newPath)
                    .ForEach(dir => Directory.Delete(dir, true));

                return new KeyValuePair<string, string>(name, tempPath);
            }
        }

        /// <summary>
        /// Get the paths for any iisnode log directories under a given path.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>Paths to any log directories.</returns>
        private static string[] GetLogDirectories(string path)
        {
            return Directory.GetDirectories(path, "*.logs", SearchOption.AllDirectories);
        }
    }
}
