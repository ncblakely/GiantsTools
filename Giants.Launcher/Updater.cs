using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Giants.Launcher
{
    public enum UpdateType
	{
		Launcher,
		Game,
	}

    public class UpdateInfo
    {
        public Version VersionFrom { get; set; }
		public Version VersionTo { get; set; }
		public Uri DownloadUri 
		{ 
			get 
			{ 
				return _downloadUri; 
			} 
			set 
			{
				_downloadUri = value;
				FileName = Path.GetFileName(value.AbsoluteUri); 
			} 
		}
		public int FileSize { get; set; }
		public string FileName { get; set; }
		public UpdateType UpdateType { get; set; }

		private Uri _downloadUri;
    }

    public class Updater
    {
		Uri _updateUri;
		Version _appVersion;
		AsyncCompletedEventHandler _updateCompletedCallback;
		DownloadProgressChangedEventHandler _updateProgressCallback;
		
		public Updater(Uri updateUri, Version appVersion)
		{
			_updateUri = updateUri;
			_appVersion = appVersion;
		}
		public void DownloadUpdateInfo(AsyncCompletedEventHandler downloadCompleteCallback, DownloadProgressChangedEventHandler downloadProgressCallback)
        {
			WebClient client = new WebClient();

			// Keep track of our progress callbacks
			_updateCompletedCallback = downloadCompleteCallback;
			_updateProgressCallback = downloadProgressCallback;

			// Download update info XML
			client.Proxy = null;
			client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCallback);
			client.DownloadDataAsync(_updateUri);
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

		private void StartGameUpdate(XElement root, Version currentVersion)
		{
			var updates = from update in root.Elements("Update")
						  select new UpdateInfo()
						  {
							  VersionFrom = new Version(update.Attribute("FromVersion").Value),
							  VersionTo = new Version(update.Attribute("ToVersion").Value),
							  DownloadUri = new Uri(update.Attribute("Url").Value),
							  UpdateType = UpdateType.Game
						  };

			// Grab the download path for the update to our current version, otherwise fall back to the full installer
			// (specially defined as FromVersion 0.0.0.0 in the XML)
			UpdateInfo info = updates.FirstOrDefault(update => update.VersionFrom == currentVersion);
			if (info == null)
				info = updates.Single(update => update.VersionFrom == new Version("0.0.0.0"));

			// Display update prompt
			string updateMsg = string.Format(Resources.UpdateAvailableText, info.VersionTo.ToString());
			if (MessageBox.Show(updateMsg, Resources.UpdateAvailableTitle, MessageBoxButtons.YesNo) == DialogResult.No)
				return; // User declined update

			string path = Path.Combine(Path.GetTempPath(), info.FileName);

			// Delete the file locally if it already exists, just to be safe
			if (File.Exists(path))
				File.Delete(path);

			info.FileSize = GetHttpFileSize(info.DownloadUri);
			if (info.FileSize == -1)
			{
				string errorMsg = string.Format(Resources.UpdateDownloadFailedText, "File not found on server.");
				MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}


            // Download the update
            WebClient client = new WebClient()
            {
                Proxy = null
            };
            client.DownloadFileAsync(info.DownloadUri, path, info);
			client.DownloadFileCompleted += _updateCompletedCallback;
			client.DownloadProgressChanged += _updateProgressCallback;
		}

		private void StartLauncherUpdate(XElement root)
		{
			var query = from update in root.Descendants("LauncherUpdate")
					   select new UpdateInfo()
					   {
						   VersionTo = new Version(update.Attribute("ToVersion").Value),
						   DownloadUri = new Uri(update.Attribute("Url").Value),
						   UpdateType = UpdateType.Launcher
					   };

			UpdateInfo info = query.FirstOrDefault();

			// Display update prompt
			string updateMsg = string.Format(Resources.LauncherUpdateAvailableText, info.VersionTo.ToString());
			if (MessageBox.Show(updateMsg, Resources.UpdateAvailableTitle, MessageBoxButtons.YesNo) == DialogResult.No)
				return; // User declined update

			string path = Path.Combine(Path.GetTempPath(), info.FileName);
			
			// Delete the file locally if it already exists, just to be safe
			if (File.Exists(path))
				File.Delete(path);

			info.FileSize = GetHttpFileSize(info.DownloadUri);
			if (info.FileSize == -1)
			{
				string errorMsg = string.Format(Resources.UpdateDownloadFailedText, "File not found on server.");
				MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

            // Download the update
            WebClient client = new WebClient()
            {
                Proxy = null
            };
            client.DownloadFileAsync(info.DownloadUri, path, info);
			client.DownloadFileCompleted += _updateCompletedCallback;
			client.DownloadProgressChanged += _updateProgressCallback;
		}

		private void DownloadDataCallback(Object sender, DownloadDataCompletedEventArgs e)
		{
			try
			{
				if (!e.Cancelled && e.Error == null)
				{
					byte[] data = (byte[])e.Result;
					string textData = System.Text.Encoding.UTF8.GetString(data);

					XElement root = XElement.Parse(textData);

					Version launcherVersion = new Version(root.Attribute("CurrentLauncherVersion").Value);
					Version gameVersion = new Version(root.Attribute("CurrentGameVersion").Value);

					Version ourVersion = new Version(Application.ProductVersion);
					if (launcherVersion > ourVersion)
					{
						StartLauncherUpdate(root);
						return;
					}
					else if (gameVersion > _appVersion)
						StartGameUpdate(root, _appVersion);
				}
			}

			catch (Exception ex)
			{
#if DEBUG
				MessageBox.Show(string.Format("Exception in DownloadDataCallback: {0}", ex.Message));
#endif
			}
		}
    }
}
