using Giants.DataContract.V1;
using Giants.Services.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Giants.Services
{
    public class VersioningService : IVersioningService
    {
        private readonly IVersioningStore versioningStore;
        private readonly IConfiguration configuration;
        private readonly ISimpleMemoryCache<VersionInfo> versionCache;
        private readonly ILogger<VersioningService> logger;

        private const string InstallerContainerName = "public";

        public VersioningService(
            ILogger<VersioningService> logger,
            IVersioningStore updaterStore,
            IConfiguration configuration,
            ISimpleMemoryCache<VersionInfo> versionCache)
        {
            this.logger = logger;
            this.versioningStore = updaterStore;
            this.configuration = configuration;
            this.versionCache = versionCache;
        }

        public async Task<VersionInfo> GetVersionInfo(string appName, string branchName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);

            branchName ??= BranchConstants.DefaultBranchName;

            var versions = await this.versionCache.GetItems();

            return versions
                .Where(x => x.AppName.Equals(appName, StringComparison.Ordinal) 
                && !string.IsNullOrEmpty(x.BranchName)
                && x.BranchName.Equals(branchName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        public async Task UpdateVersionInfo(string appName, AppVersion appVersion, string fileName, string branchName, bool force)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);
            ArgumentUtility.CheckForNull(appVersion);
            ArgumentUtility.CheckStringForNullOrEmpty(fileName);
            ArgumentUtility.CheckStringForNullOrEmpty(branchName);

            var storageAccountUri = new Uri(this.configuration["StorageAccountUri"]);

            VersionInfo versionInfo = await this.GetVersionInfo(appName, branchName);
            if (versionInfo == null)
            {
                throw new ArgumentException($"No version information for {appName} ({branchName}) found.");
            }

            if (!force && (appVersion < versionInfo.Version))
            {
                throw new ArgumentException($"Version {appVersion.SerializeToJson()} is less than current version {versionInfo.Version.SerializeToJson()}", nameof(appVersion));
            }

            if (fileName.Contains('/') || fileName.Contains('\\'))
            {
                throw new ArgumentException("File name must be relative to configured storage account.", nameof(fileName));
            }

            var installerUri = new Uri(storageAccountUri, $"{InstallerContainerName}/{fileName}");
            var newVersionInfo = new VersionInfo()
            {
                AppName = appName,
                Version = appVersion,
                InstallerUri = installerUri,
                BranchName = branchName,
            };

            this.logger.LogInformation("Updating version info for {appName}: {versionInfo}", appName, newVersionInfo.SerializeToJson());

            await this.versioningStore.UpdateVersionInfo(newVersionInfo);
            this.versionCache.Invalidate();
        }

        public async Task<IEnumerable<string>> GetBranches(string appName)
        {
            var allVersions = await this.versionCache.GetItems();

            return allVersions
                .Where(x => x.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.BranchName).ToList();
        }
    }
}
