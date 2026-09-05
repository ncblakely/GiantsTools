using System;
using System.Text.RegularExpressions;
using Giants.WebApi.Clients;

namespace Giants.Launcher
{
    internal static class InstallerValidation
    {
        private static readonly Uri TrustedInstallerBaseUri = new Uri("https://giants.blob.core.windows.net/public/");
        private static readonly Regex InstallerFileNamePattern = new Regex(
            @"^GPatch_[0-9]+_[0-9]+_[0-9]+_[0-9]+\.exe$",
            RegexOptions.CultureInvariant);

        internal static bool TryGetTrustedInstallerUri(VersionInfo versionInfo, out Uri installerUri)
        {
            installerUri = null;
            if (versionInfo == null)
            {
                return false;
            }

            if (IsValidInstallerFileName(versionInfo.InstallerPath, versionInfo.Version))
            {
                installerUri = new Uri(TrustedInstallerBaseUri, Uri.EscapeDataString(versionInfo.InstallerPath));
                return true;
            }

            if (IsTrustedInstallerUri(versionInfo.InstallerUri, versionInfo.Version))
            {
                installerUri = versionInfo.InstallerUri;
                return true;
            }

            return false;
        }

        internal static bool IsMatchingSha256(string expectedSha256, byte[] actualSha256)
        {
            string actual = BitConverter.ToString(actualSha256).Replace("-", string.Empty);
            return string.Equals(expectedSha256, actual, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsValidSha256(string sha256)
        {
            if (string.IsNullOrEmpty(sha256) || sha256.Length != 64)
            {
                return false;
            }

            foreach (char character in sha256)
            {
                if (!Uri.IsHexDigit(character))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsValidInstallerFileName(string fileName, AppVersion version)
        {
            return !string.IsNullOrEmpty(fileName)
                && InstallerFileNamePattern.IsMatch(fileName)
                && (version == null
                    || string.Equals(
                        fileName,
                        $"GPatch_{version.Major}_{version.Minor}_{version.Build}_{version.Revision}.exe",
                        StringComparison.Ordinal));
        }

        private static bool IsTrustedInstallerUri(Uri installerUri, AppVersion version)
        {
            if (installerUri == null
                || !installerUri.IsAbsoluteUri
                || !string.Equals(installerUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(installerUri.Host, TrustedInstallerBaseUri.Host, StringComparison.OrdinalIgnoreCase)
                || installerUri.Port != TrustedInstallerBaseUri.Port
                || !string.IsNullOrEmpty(installerUri.Query)
                || !string.IsNullOrEmpty(installerUri.Fragment))
            {
                return false;
            }

            string[] segments = installerUri.Segments;
            if (segments.Length != 3
                || !string.Equals(segments[1].TrimEnd('/'), "public", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string fileName = Uri.UnescapeDataString(segments[2].TrimEnd('/'));
            return IsValidInstallerFileName(fileName, version)
                && string.Equals(
                    installerUri.AbsoluteUri,
                    new Uri(TrustedInstallerBaseUri, Uri.EscapeDataString(fileName)).AbsoluteUri,
                    StringComparison.OrdinalIgnoreCase);
        }

    }
}
