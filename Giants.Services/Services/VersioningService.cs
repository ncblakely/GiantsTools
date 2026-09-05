using Giants.DataContract.V1;
using Giants.Services.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const string InstallerFileNamePattern = @"^GPatch_[0-9]+_[0-9]+_[0-9]+_[0-9]+\.exe$";

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
                .Where(x => string.Equals(x?.AppName, appName, StringComparison.Ordinal)
                    && !string.IsNullOrEmpty(x.BranchName)
                    && x.BranchName.Equals(branchName, StringComparison.OrdinalIgnoreCase))
                .Select(this.NormalizeVersionInfo)
                .FirstOrDefault(x => x != null);
        }

        public async Task UpdateVersionInfo(
            string appName,
            AppVersion appVersion,
            string fileName,
            string branchName,
            bool force,
            string installerSha256 = null,
            string installerSignature = null)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);
            ArgumentUtility.CheckForNull(appVersion);
            ArgumentUtility.CheckStringForNullOrEmpty(fileName);
            ArgumentUtility.CheckStringForNullOrEmpty(branchName);

            var storageAccountUri = new Uri(this.configuration["StorageAccountUri"], UriKind.Absolute);

            // Read the raw record so a malformed record can be repaired through the authenticated write path.
            var versions = await this.versionCache.GetItems();
            VersionInfo versionInfo = versions
                .Where(x => string.Equals(x?.AppName, appName, StringComparison.Ordinal)
                    && string.Equals(x?.BranchName, branchName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (versionInfo == null)
            {
                throw new ArgumentException($"No version information for {appName} ({branchName}) found.");
            }

            if (!force && versionInfo.Version != null && (appVersion < versionInfo.Version))
            {
                throw new ArgumentException($"Version {appVersion.SerializeToJson()} is less than current version {versionInfo.Version.SerializeToJson()}", nameof(appVersion));
            }

            if (!this.IsValidInstallerFileName(fileName, appVersion))
            {
                throw new ArgumentException("File name must be a GPatch versioned executable.", nameof(fileName));
            }

            if (!string.IsNullOrEmpty(installerSha256)
                && !this.IsValidSha256(installerSha256))
            {
                throw new ArgumentException("Installer SHA-256 must contain 64 hexadecimal characters.", nameof(installerSha256));
            }

            if (!string.IsNullOrEmpty(installerSignature)
                && (!this.IsValidSignature(installerSignature)
                    || !this.IsValidSha256(installerSha256)))
            {
                throw new ArgumentException(
                    "Installer signatures require a valid RSA signature and installer SHA-256.",
                    nameof(installerSignature));
            }

            var newVersionInfo = new VersionInfo()
            {
                AppName = appName,
                Version = appVersion,
                InstallerUri = this.CreateInstallerUri(storageAccountUri, fileName),
                InstallerPath = fileName,
                InstallerSha256 = installerSha256?.ToLowerInvariant(),
                InstallerSignature = installerSignature,
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
                .Select(this.NormalizeVersionInfo)
                .Where(x => x != null && x.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.BranchName).ToList();
        }

        private VersionInfo NormalizeVersionInfo(VersionInfo versionInfo)
        {
            if (versionInfo == null
                || string.IsNullOrEmpty(versionInfo.AppName)
                || string.IsNullOrEmpty(versionInfo.BranchName)
                || versionInfo.Version == null)
            {
                return null;
            }

            var storageAccountUri = new Uri(this.configuration["StorageAccountUri"], UriKind.Absolute);

            if (this.IsAboutBlank(versionInfo.InstallerUri))
            {
                return new VersionInfo()
                {
                    AppName = versionInfo.AppName,
                    Version = versionInfo.Version,
                    InstallerUri = new Uri("about:blank"),
                    BranchName = versionInfo.BranchName,
                };
            }

            string installerFileName = versionInfo.InstallerPath;
            if (!this.IsValidInstallerFileName(installerFileName, versionInfo.Version)
                && !this.TryGetLegacyInstallerFileName(versionInfo.InstallerUri, storageAccountUri, versionInfo.Version, out installerFileName))
            {
                this.logger.LogWarning(
                    "Ignoring invalid installer metadata for {appName} ({branchName}).",
                    versionInfo.AppName,
                    versionInfo.BranchName);
                return null;
            }

            if (!string.IsNullOrEmpty(versionInfo.InstallerSignature)
                && (!this.IsValidSignature(versionInfo.InstallerSignature)
                    || !this.IsValidSha256(versionInfo.InstallerSha256)))
            {
                this.logger.LogWarning(
                    "Ignoring update metadata with an invalid installer signature for {appName} ({branchName}).",
                    versionInfo.AppName,
                    versionInfo.BranchName);
                return null;
            }

            return new VersionInfo()
            {
                AppName = versionInfo.AppName,
                Version = versionInfo.Version,
                InstallerUri = this.CreateInstallerUri(storageAccountUri, installerFileName),
                InstallerPath = installerFileName,
                InstallerSha256 = this.IsValidSha256(versionInfo.InstallerSha256)
                    ? versionInfo.InstallerSha256.ToLowerInvariant()
                    : null,
                InstallerSignature = versionInfo.InstallerSignature,
                BranchName = versionInfo.BranchName,
            };
        }

        private bool TryGetLegacyInstallerFileName(
            Uri installerUri,
            Uri storageAccountUri,
            AppVersion version,
            out string fileName)
        {
            fileName = null;

            if (installerUri == null
                || !installerUri.IsAbsoluteUri
                || !string.Equals(installerUri.Scheme, storageAccountUri.Scheme, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(installerUri.Host, storageAccountUri.Host, StringComparison.OrdinalIgnoreCase)
                || installerUri.Port != storageAccountUri.Port
                || !string.IsNullOrEmpty(installerUri.Query)
                || !string.IsNullOrEmpty(installerUri.Fragment))
            {
                return false;
            }

            string[] segments = installerUri.Segments;
            if (segments.Length != 3
                || !string.Equals(segments[1].TrimEnd('/'), InstallerContainerName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fileName = Uri.UnescapeDataString(segments[2].TrimEnd('/'));
            return this.IsValidInstallerFileName(fileName, version)
                && string.Equals(
                    installerUri.AbsoluteUri,
                    this.CreateInstallerUri(storageAccountUri, fileName).AbsoluteUri,
                    StringComparison.OrdinalIgnoreCase);
        }

        private Uri CreateInstallerUri(Uri storageAccountUri, string fileName)
        {
            return new Uri(storageAccountUri, $"{InstallerContainerName}/{Uri.EscapeDataString(fileName)}");
        }

        private bool IsValidInstallerFileName(string fileName, AppVersion version)
        {
            return !string.IsNullOrEmpty(fileName)
                && Regex.IsMatch(fileName, InstallerFileNamePattern, RegexOptions.CultureInvariant)
                && string.Equals(
                    fileName,
                    $"GPatch_{version.Major}_{version.Minor}_{version.Build}_{version.Revision}.exe",
                    StringComparison.Ordinal);
        }

        private bool IsValidSha256(string sha256)
        {
            return !string.IsNullOrEmpty(sha256)
                && sha256.Length == 64
                && sha256.All(Uri.IsHexDigit);
        }

        private bool IsValidSignature(string signature)
        {
            if (string.IsNullOrEmpty(signature)
                || signature.Length > 4096
                || signature.Any(c => !((c >= 'A' && c <= 'Z')
                    || (c >= 'a' && c <= 'z')
                    || (c >= '0' && c <= '9')
                    || c == '-'
                    || c == '_')))
            {
                return false;
            }

            try
            {
                string paddedSignature = signature
                    .Replace('-', '+')
                    .Replace('_', '/');
                paddedSignature += new string('=', (4 - paddedSignature.Length % 4) % 4);
                return Convert.FromBase64String(paddedSignature).Length == 256;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private bool IsAboutBlank(Uri installerUri)
        {
            return installerUri != null
                && string.Equals(installerUri.AbsoluteUri, "about:blank", StringComparison.OrdinalIgnoreCase);
        }
    }
}
