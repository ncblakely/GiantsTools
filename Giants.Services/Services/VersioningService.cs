using Giants.DataContract.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Giants.Services
{
    public class VersioningService : IVersioningService
    {
        private readonly IVersioningStore versioningStore;
        private readonly IConfiguration configuration;
        private readonly ILogger<VersioningService> logger;
        private const string InstallerContainerName = "public";

        public VersioningService(
            IVersioningStore updaterStore,
            IConfiguration configuration,
            ILogger<VersioningService> logger)
        {
            this.versioningStore = updaterStore;
            this.configuration = configuration;
            this.logger = logger;
        }

        public Task<VersionInfo> GetVersionInfo(string appName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);

            return this.versioningStore.GetVersionInfo(appName);
        }

        public async Task UpdateVersionInfo(string appName, AppVersion appVersion, string fileName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);
            ArgumentUtility.CheckForNull(appVersion);
            ArgumentUtility.CheckStringForNullOrEmpty(fileName);

            Uri storageAccountUri = new Uri(this.configuration["StorageAccountUri"]);

            VersionInfo versionInfo = await this.GetVersionInfo(appName);
            if (versionInfo == null)
            {
                throw new ArgumentException($"No version information for {appName} found.", nameof(appName));
            }

            if (appVersion < versionInfo.Version)
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
            };

            this.logger.LogInformation("Updating version info for {appName}: {versionInfo}", appName, newVersionInfo.SerializeToJson());

            await this.versioningStore.UpdateVersionInfo(newVersionInfo);
        }
    }
}
