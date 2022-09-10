using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giants.WebApi.Clients;
using Microsoft.Win32;

namespace Giants.Launcher
{
    public partial class LauncherForm : Form
    {
        // Constant settings
        private const string GamePath = "GiantsMain.exe";
		private const string RegistryKey = @"HKEY_CURRENT_USER\Software\PlanetMoon\Giants";
		private const string RegistryValue = "DestDir";

		private readonly HttpClient httpClient;
		private readonly BranchesClient branchHttpClient;
		private readonly VersionClient versionHttpClient;
        private readonly CommunityClient communityHttpClient;

        private string commandLine;
		private string gamePath = null;
		private Updater updater;
		private readonly Config config;
		private string branchName;
        private string communityAppUri;

        public LauncherForm()
        {
			this.InitializeComponent();
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// Set window title
			this.SetTitle();

            this.updater = new Updater(
            updateCompletedCallback: this.LauncherForm_DownloadCompletedCallback,
            updateProgressCallback: this.LauncherForm_DownloadProgressCallback);

            // Read newer file-based game settings
            this.config = new Config();
			this.config.Read();

			this.config.TryGetString(ConfigSections.Network, ConfigKeys.MasterServerHostName, ConfigDefaults.MasterServerHostNameDefault, out string baseUrl);
            this.config.TryGetString(ConfigSections.Update, ConfigKeys.BranchName, defaultValue: ConfigDefaults.BranchNameDefault, out string branchName);

			this.branchName = branchName;

            this.httpClient = new HttpClient(
				new HttpClientHandler() 
				{ 
					UseProxy = false
				});
			this.branchHttpClient = new BranchesClient(this.httpClient)
			{
				BaseUrl = baseUrl,
				
			};
            this.versionHttpClient = new VersionClient(this.httpClient)
            {
                BaseUrl = baseUrl
			};
            this.communityHttpClient = new CommunityClient(this.httpClient)
            {
                BaseUrl = baseUrl
			};
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.config.Write();
            Application.Exit();
        }

		private void btnPlay_Click(object sender, EventArgs e)
		{
			GameSettings.Save();
			this.config.Write();

			foreach (string c in Environment.GetCommandLineArgs())
			{
                this.commandLine = this.commandLine + c + " ";
			}

            string commandLine = string.Format("{0} -launcher", this.commandLine.Trim());

			try
			{
				Process gameProcess = new Process();

                gameProcess.StartInfo.Arguments = commandLine;
				gameProcess.StartInfo.FileName = this.gamePath;
				gameProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(this.gamePath);
	
				gameProcess.Start();
				Application.Exit();
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Failed to launch game process at: {0}. {1}", this.gamePath, ex.Message),
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

        private async void btnOptions_Click(object sender, EventArgs e)
        {
			OptionsForm form = new OptionsForm(
				title: Resources.AppName + " Options",
				gamePath: this.gamePath,
				appName: ApplicationNames.Giants,
				currentBranchName: this.branchName,
				config: this.config,
				branchesClient: this.branchHttpClient)
			{
				StartPosition = FormStartPosition.CenterParent
			};

			form.ShowDialog();

            this.config.TryGetBool(ConfigSections.Update, ConfigKeys.EnableBranchSelection, defaultValue: false, out bool enableBranchSelection);
			if (enableBranchSelection)
			{
				this.config.TryGetString(ConfigSections.Update, ConfigKeys.BranchName, defaultValue: ConfigDefaults.BranchNameDefault, out string branchName);

				if (!this.branchName.Equals(branchName))
				{
					this.branchName = branchName;

                    VersionInfo gameVersionInfo = await this.GetVersionInfo(
						GetApplicationName(ApplicationType.Game), this.branchName);

                    this.btnPlay.Enabled = false;
                    await this.updater.UpdateApplication(ApplicationType.Game, gameVersionInfo);
				}
			}
        }

        private async void LauncherForm_Load(object sender, EventArgs e)
        {
            // Find the game executable, first looking for it relative to our current directory and then
            // using the registry path if that fails.
            this.gamePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + GamePath;
            if (!File.Exists(this.gamePath))
            {
                this.gamePath = (string)Registry.GetValue(RegistryKey, RegistryValue, null);
                if (this.gamePath != null)
                    this.gamePath = Path.Combine(this.gamePath, GamePath);

                if (this.gamePath == null || !File.Exists(this.gamePath))
                {
                    string message = string.Format(Resources.AppNotFound, Resources.AppName);
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
					return;
                }
            }

			// Read game settings from registry
			GameSettings.Load(this.gamePath);

			if (GameSettings.Get<int>("NoAutoUpdate") == 0)
			{
                Task updateTask = this.CheckForUpdates();
                Task discordTask = this.UpdateDiscordStatus();

                await Task.WhenAll(updateTask, discordTask);
			}
		}

		private async Task UpdateDiscordStatus()
        {
			try
			{
				var status = await this.communityHttpClient.GetDiscordStatusAsync();
				
				this.communityAppUri = status.CommunityAppUri;
				this.CommunityLabel.Text = string.Format(Resources.CommunityLabel, status.CommunityAppName);
				this.CommunityLabel.Visible = true;
			}
			catch (Exception)
            {
				// Ignore
            }
        }

		private async Task CheckForUpdates()
        {
            Task<VersionInfo> gameVersionInfo = this.GetVersionInfo(
				GetApplicationName(ApplicationType.Game), this.branchName);

            Task<VersionInfo> launcherVersionInfo = this.GetVersionInfo(
                GetApplicationName(ApplicationType.Launcher), this.branchName);

            Version localGameVersion = VersionHelper.GetGameVersion(this.gamePath);
            Version localLauncherVersion = VersionHelper.GetLauncherVersion();

            await Task.WhenAll(gameVersionInfo, launcherVersionInfo);

            if (this.updater.IsUpdateRequired(ApplicationType.Game, gameVersionInfo.Result, localGameVersion))
			{
				this.btnPlay.Enabled = false;
				await this.updater.UpdateApplication(ApplicationType.Game, gameVersionInfo.Result);
			}

            if (this.updater.IsUpdateRequired(ApplicationType.Launcher, launcherVersionInfo.Result, localLauncherVersion))
            {
                this.btnPlay.Enabled = false;
                await this.updater.UpdateApplication(ApplicationType.Launcher, launcherVersionInfo.Result);
            }
        }

		private static string GetApplicationName(ApplicationType applicationType)
        {
			switch (applicationType)
            {
				case ApplicationType.Game:
					return ApplicationNames.Giants;
                case ApplicationType.Launcher:
                    return ApplicationNames.GiantsLauncher;
            }

            throw new ArgumentOutOfRangeException();
        }

        private async Task<VersionInfo> GetVersionInfo(string appName, string branchName)
        {
			VersionInfo versionInfo;
			try
			{
				versionInfo = await this.versionHttpClient.GetVersionInfoAsync(appName, branchName);
				return versionInfo;
			}
			catch (ApiException ex)
			{
				MessageBox.Show($"Exception retrieving version information: {ex.StatusCode}");
				return null;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Exception retrieving version information: {ex.Message}");
				return null;
			}
		}

        private void LauncherForm_MouseDown(object sender, MouseEventArgs e)
		{
			// Force window to be draggable even though we have no menu bar
			if (e.Button == MouseButtons.Left)
			{
				NativeMethods.ReleaseCapture();
				NativeMethods.SendMessage(this.Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
			}
		}

		private void LauncherForm_Shown(object sender, EventArgs e)
		{
            this.btnOptions.Visible = true;
            this.btnPlay.Visible = true;
            this.btnExit.Visible = true;

			// Play intro sound
			SoundPlayer player = new SoundPlayer(Resources.LauncherStart);
			player.Play();
		}

		private void LauncherForm_DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			this.btnPlay.Enabled = true;

			if (e.Cancelled)
			{
				return;
			}

            this.updateProgressBar.Value = 0;
            this.updateProgressBar.Visible = false;
            this.txtProgress.Visible = false;

			if (e.Error != null)
			{
				string errorMsg = string.Format(Resources.UpdateDownloadFailedText, e.Error.Message);
				MessageBox.Show(errorMsg, Resources.UpdateDownloadFailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else
			{
				// Show "Download Complete" message, warn that we're closing
				MessageBox.Show(Resources.LauncherClosingText, Resources.LauncherClosingTitle);

				UpdateInfo updateInfo = (UpdateInfo)e.UserState;

				// Start the installer process
				Process updaterProcess = new Process();
				updaterProcess.StartInfo.FileName = updateInfo.FilePath;
				updaterProcess.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

				if (updateInfo.ApplicationType == ApplicationType.Game)
				{
					// Default installation directory to current directory
					updaterProcess.StartInfo.Arguments = string.Format("/D {0}", Path.GetDirectoryName(Application.ExecutablePath));
				}
				else if (updateInfo.ApplicationType == ApplicationType.Launcher)
				{
					// Default installation directory to current directory and launch a silent install
					updaterProcess.StartInfo.Arguments = string.Format("/S /D {0}", Path.GetDirectoryName(Application.ExecutablePath));
				}

				updaterProcess.Start();

                this.config.TryGetBool(ConfigSections.Update, ConfigKeys.EnableBranchSelection, defaultValue: false, out bool enableBranchSelection);
                if (enableBranchSelection)
				{
                    this.config.SetValue(ConfigSections.Update, ConfigKeys.BranchName, this.branchName);
                }

				this.config.Write();

				Application.Exit();
				return;
			}
		}

		private void LauncherForm_DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
		{
            this.updateProgressBar.Visible = true;
            this.updateProgressBar.Value = e.ProgressPercentage;

			UpdateInfo info = (UpdateInfo)e.UserState;

            this.txtProgress.Visible = true;
            this.txtProgress.Text = string.Format(Resources.DownloadProgress, e.ProgressPercentage, info.FileSize / 1024 / 1024);
		}

        private void DiscordLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			if (string.IsNullOrEmpty(this.communityAppUri))
            {
				return;
            }

			var uri = new Uri(this.communityAppUri);
			if (uri.Scheme != "https")
            {
				// For security, reject any non-HTTPS or local file system URIs
				return;
            }

			Process.Start(this.communityAppUri);
        }

		private void SetTitle()
        {
			string title = Resources.AppName;
			this.Text = title;
        }
    }
}
