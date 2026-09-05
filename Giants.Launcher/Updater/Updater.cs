using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giants.WebApi.Clients;

namespace Giants.Launcher
{
    public class Updater
    {
        private readonly AsyncCompletedEventHandler updateCompletedCallback;
        private readonly EventHandler<UpdateProgressEventArgs> updateProgressCallback;
        private readonly HttpClient httpClient;

        public Updater(
            AsyncCompletedEventHandler updateCompletedCallback,
            EventHandler<UpdateProgressEventArgs> updateProgressCallback)
        {
            this.updateCompletedCallback = updateCompletedCallback;
            this.updateProgressCallback = updateProgressCallback;
            this.httpClient = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseProxy = false,
            });
        }

        public bool IsUpdateRequired(ApplicationType applicationType, VersionInfo remoteVersionInfo, Version localVersion)
        {
            if (remoteVersionInfo == null
                || remoteVersionInfo.Version == null
                || localVersion == null
                || !InstallerValidation.TryGetTrustedInstallerUri(remoteVersionInfo, out _)
                || !InstallerSignatureValidation.IsTrusted(remoteVersionInfo))
            {
                return false;
            }

            Version remoteVersion = this.ToVersion(remoteVersionInfo.Version);
            if (remoteVersion <= localVersion)
            {
                return false;
            }

            // Display update prompt
            string updateMsg = applicationType == ApplicationType.Game ?
                                string.Format(Resources.UpdateAvailableText, remoteVersion) :
                                string.Format(Resources.LauncherUpdateAvailableText, remoteVersion);

            return MessageBox.Show(
                updateMsg,
                Resources.UpdateAvailableTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information) == DialogResult.Yes;
        }

        public async Task UpdateApplication(ApplicationType applicationType, VersionInfo versionInfo)
        {
            try
            {
                await this.StartApplicationUpdate(applicationType, versionInfo);
            }
            catch (Exception e)
            {
                this.updateCompletedCallback(
                    this,
                    new AsyncCompletedEventArgs(e, false, null));
            }
        }

        private async Task StartApplicationUpdate(ApplicationType applicationType, VersionInfo versionInfo)
        {
            if (!InstallerValidation.TryGetTrustedInstallerUri(versionInfo, out Uri installerUri))
            {
                throw new InvalidOperationException("The update location is not a trusted installer URL.");
            }

            if (!InstallerSignatureValidation.IsTrusted(versionInfo))
            {
                throw new InvalidOperationException("The installer signature is invalid.");
            }

            string patchFileName = Path.GetFileName(Uri.UnescapeDataString(installerUri.LocalPath));
            string localPath = Path.Combine(Path.GetTempPath(), patchFileName);

            // Delete the file locally if it already exists, just to be safe
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            try
            {
                using (HttpResponseMessage response = await this.httpClient.GetAsync(
                    installerUri,
                    HttpCompletionOption.ResponseHeadersRead))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new InvalidOperationException($"Installer download returned HTTP {(int)response.StatusCode}.");
                    }

                    long fileSize = response.Content.Headers.ContentLength ?? -1;
                    if (fileSize <= 0)
                    {
                        throw new InvalidOperationException("Installer size was missing or invalid.");
                    }

                    var updateInfo = new UpdateInfo()
                    {
                        FilePath = localPath,
                        FileSize = fileSize,
                        ApplicationType = applicationType,
                    };

                    using (Stream input = await response.Content.ReadAsStreamAsync())
                    using (FileStream output = File.Create(localPath))
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] buffer = new byte[81920];
                        long bytesReceived = 0;
                        int bytesRead;

                        while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                            bytesReceived += bytesRead;

                            int progressPercentage = (int)Math.Min(100, bytesReceived * 100 / fileSize);
                            this.updateProgressCallback(
                                this,
                                new UpdateProgressEventArgs(
                                    progressPercentage,
                                    bytesReceived,
                                    fileSize,
                                    updateInfo));
                        }

                        sha256.TransformFinalBlock(new byte[0], 0, 0);

                        if (bytesReceived != fileSize)
                        {
                            throw new InvalidOperationException("Installer download was incomplete.");
                        }

                        if (!string.IsNullOrEmpty(versionInfo.InstallerSha256)
                            && !InstallerValidation.IsMatchingSha256(versionInfo.InstallerSha256, sha256.Hash))
                        {
                            throw new InvalidOperationException("Installer checksum validation failed.");
                        }
                    }

                    this.updateCompletedCallback(
                        this,
                        new AsyncCompletedEventArgs(null, false, updateInfo));
                }
            }
            catch
            {
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }

                throw;
            }
        }

        private Version ToVersion(AppVersion version)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
