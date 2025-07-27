using Giants.WebApi.Clients;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giants.Launcher
{
    public class DownloadProgressInfo
    {
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
        public int ProgressPercentage { get; set; }
        public object UserState { get; set; }
    }

    public class Updater : IDisposable
    {
        private const int DownloadBufferSize = 8192;

        private readonly AsyncCompletedEventHandler updateCompletedCallback;
        private readonly Action<DownloadProgressInfo> updateProgressCallback;
        private readonly string patchServerHostName;
        private readonly HttpClient httpClient;
        private bool disposed = false;

        public Updater(
            AsyncCompletedEventHandler updateCompletedCallback,
            Action<DownloadProgressInfo> updateProgressCallback,
            string patchServerHostName)
        {
            this.updateCompletedCallback = updateCompletedCallback;
            this.updateProgressCallback = updateProgressCallback;
            this.patchServerHostName = patchServerHostName;

            var handler = new HttpClientHandler
            {
                CheckCertificateRevocationList = true
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            this.httpClient = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            // Add security headers
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Giants.Launcher");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                if (this.httpClient != null)
                {
                    this.httpClient.Dispose();
                }
                disposed = true;
            }
        }

        public bool IsUpdateRequired(ApplicationType applicationType, VersionInfo remoteVersionInfo, Version localVersion)
        {
            if (remoteVersionInfo != null && remoteVersionInfo.InstallerUri != null
                && this.ToVersion(remoteVersionInfo.Version) > localVersion)
            {
                // Display update prompt
                string updateMsg = applicationType == ApplicationType.Game ?
                                    string.Format(Resources.UpdateAvailableText, this.ToVersion(remoteVersionInfo.Version).ToString()) :
                                    string.Format(Resources.LauncherUpdateAvailableText, this.ToVersion(remoteVersionInfo.Version).ToString());

                if (MessageBox.Show(updateMsg, Resources.UpdateAvailableTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task UpdateApplication(ApplicationType applicationType, VersionInfo versionInfo)
        {
            try
            {
                await this.StartApplicationUpdate(applicationType, versionInfo);
            }
            catch (Exception e)
            {
                string errorMsg = string.Format(Resources.UpdateDownloadFailedText, e.Message);
                MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> GetHttpFileSize(Uri uri)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
                using (var response = await this.httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength.HasValue)
                    {
                        return (int)response.Content.Headers.ContentLength.Value;
                    }
                }
            }
            catch (Exception e)
            {
                string errorMsg = string.Format(Resources.UpdateDownloadFailedText, e.Message);
                MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return -1;
        }

        private async Task StartApplicationUpdate(ApplicationType applicationType, VersionInfo versionInfo)
        {
            String absoluteUri = versionInfo.InstallerUri.AbsoluteUri;
            string patchFileName = Path.GetFileName(absoluteUri);

            if (!IsValidPatchUri(absoluteUri))
            {
                throw new SecurityException($"Invalid patch download URL detected: {patchFileName}");
            }

            string localPath = Path.Combine(Path.GetTempPath(), patchFileName);

            // Delete the file locally if it already exists, just to be safe
            DeleteFileIfExists(localPath);

            int fileSize = await this.GetHttpFileSize(versionInfo.InstallerUri);
            if (fileSize == -1)
            {
                string errorMsg = string.Format(Resources.UpdateDownloadFailedText, "File not found on server.");
                MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var updateInfo = new UpdateInfo()
            {
                FilePath = localPath,
                FileSize = fileSize,
                ApplicationType = applicationType
            };

            // Download the update
            await DownloadFileWithProgress(versionInfo.InstallerUri, localPath, updateInfo);
        }

        private async Task DownloadFileWithProgress(Uri downloadUri, string localPath, UpdateInfo updateInfo)
        {
            try
            {
                using (var response = await this.httpClient.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1 && this.updateProgressCallback != null;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, DownloadBufferSize, true))
                    {
                        var buffer = new byte[DownloadBufferSize];
                        var totalBytesRead = 0L;
                        var lastReportedPercentage = -1;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (canReportProgress)
                            {
                                var progressPercentage = (int)((totalBytesRead * 100L) / totalBytes);

                                // Only report progress when percentage changes to avoid overwhelming UI thread
                                if (progressPercentage != lastReportedPercentage)
                                {
                                    lastReportedPercentage = progressPercentage;

                                    var progressInfo = new DownloadProgressInfo
                                    {
                                        BytesReceived = totalBytesRead,
                                        TotalBytes = totalBytes,
                                        ProgressPercentage = progressPercentage,
                                        UserState = updateInfo
                                    };

                                    this.updateProgressCallback(progressInfo);
                                }
                            }
                        }
                    }
                }

                // Notify completion
                var completedArgs = new AsyncCompletedEventArgs(null, false, updateInfo);
                if (this.updateCompletedCallback != null)
                {
                    this.updateCompletedCallback(this, completedArgs);
                }
            }
            catch (Exception ex)
            {
                // Clean up partial download
                DeleteFileIfExists(localPath);

                // Notify completion with error
                var errorArgs = new AsyncCompletedEventArgs(ex, false, updateInfo);
                if (this.updateCompletedCallback != null)
                {
                    this.updateCompletedCallback(this, errorArgs);
                }
            }
        }

        private Version ToVersion(AppVersion version)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }

        private static void DeleteFileIfExists(string localPath)
        {
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }
        }

        private bool IsValidPatchUri(string absoluteUri)
        {
            // Download location should be from a known domain
            if (!absoluteUri.StartsWith(this.patchServerHostName))
                return false;

            // Reject local file system paths
            if (Path.IsPathRooted(absoluteUri))
                return false;

            string fileName = Path.GetFileName(absoluteUri);

            // Check file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            string[] AllowedExtensions = { ".exe", ".msi" };
            if (!Array.Exists(AllowedExtensions, ext => ext == extension))
                return false;

            return true;
        }
    }
}
