﻿using Giants.WebApi.Clients;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giants.Launcher
{
    public class Updater
    {
        private readonly AsyncCompletedEventHandler updateCompletedCallback;
        private readonly DownloadProgressChangedEventHandler updateProgressCallback;

        public Updater(
            AsyncCompletedEventHandler updateCompletedCallback,
            DownloadProgressChangedEventHandler updateProgressCallback)
        {
            this.updateCompletedCallback = updateCompletedCallback;
            this.updateProgressCallback = updateProgressCallback;
        }

        public bool IsUpdateRequired(ApplicationType applicationType, VersionInfo remoteVersionInfo, Version localVersion)
        {
            if (remoteVersionInfo?.InstallerUri != null 
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

        public Task UpdateApplication(ApplicationType applicationType, VersionInfo versionInfo)
        {
            try
            {
                this.StartApplicationUpdate(applicationType, versionInfo);
            }
            catch (Exception e)
            {
                string errorMsg = string.Format(Resources.UpdateDownloadFailedText, e.Message);
                MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return Task.CompletedTask;
        }

        private int GetHttpFileSize(Uri uri)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(uri);
            req.Proxy = null;
            req.Method = "HEAD";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK && int.TryParse(resp.Headers.Get("Content-Length"), out int contentLength))
            {
                resp.Close();
                return contentLength;
            }

            resp.Close();
            return -1;

        }

        private void StartApplicationUpdate(ApplicationType applicationType, VersionInfo versionInfo)
        {
            string patchFileName = Path.GetFileName(versionInfo.InstallerUri.AbsoluteUri);
            string localPath = Path.Combine(Path.GetTempPath(), patchFileName);

            // Delete the file locally if it already exists, just to be safe
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            int fileSize = this.GetHttpFileSize(versionInfo.InstallerUri);
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
            // TODO: Super old code, replace this with async HttpClient
            WebClient client = new WebClient();
            client.DownloadFileAsync(versionInfo.InstallerUri, localPath, updateInfo);
            client.DownloadFileCompleted += this.updateCompletedCallback;
            client.DownloadProgressChanged += this.updateProgressCallback;
        }

        private Version ToVersion(AppVersion version)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
