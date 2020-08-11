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
        private const string GAME_NAME = "Giants: Citizen Kabuto";
        private const string GAME_PATH = "GiantsMain.exe";
		private const string REGISTRY_KEY = @"HKEY_CURRENT_USER\Software\PlanetMoon\Giants";
		private const string REGISTRY_VALUE = "DestDir";
        private readonly VersionClient httpClient;

        private string commandLine = String.Empty;
		private string gamePath = null;
		private Updater updater;

        public LauncherForm()
        {
			this.InitializeComponent();

			// Set window title
			this.Text = GAME_NAME;

			this.httpClient = new VersionClient(new HttpClient());
			this.httpClient.BaseUrl = "https://giants.azurewebsites.net"; // TODO: Read from file
		}

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

		private void btnPlay_Click(object sender, EventArgs e)
		{
			GameSettings.Save();

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
			catch(Exception ex)
			{
				MessageBox.Show(string.Format("Failed to launch game process at: {0}. {1}", this.gamePath, ex.Message),
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

        private void btnOptions_Click(object sender, EventArgs e)
        {
            OptionsForm form = new OptionsForm(GAME_NAME + " Options", this.gamePath);

			form.ShowDialog();
        }

        private async void LauncherForm_Load(object sender, EventArgs e)
        {
            // Find the game executable, first looking for it relative to our current directory and then
            // using the registry path if that fails.
            this.gamePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + GAME_PATH;
            if (!File.Exists(this.gamePath))
            {
                this.gamePath = (string)Registry.GetValue(REGISTRY_KEY, REGISTRY_VALUE, null);
                if (this.gamePath != null)
                    this.gamePath = Path.Combine(this.gamePath, GAME_PATH);

                if (this.gamePath == null || !File.Exists(this.gamePath))
                {
                    string message = string.Format(Resources.AppNotFound, GAME_NAME);
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
					return;
                }
            }

			// Read game settings from registry
			GameSettings.Load(this.gamePath);

			if ((int)GameSettings.Get("NoAutoUpdate") == 0)
			{
				Version gameVersion = VersionHelper.GetGameVersion(this.gamePath);
				if (gameVersion == null)
				{
					string message = string.Format(Resources.AppNotFound, GAME_NAME);
					MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Application.Exit();
				}

				var appVersions = new Dictionary<ApplicationType, Version>()
				{
					[ApplicationType.Game] = gameVersion,
					[ApplicationType.Launcher] = VersionHelper.GetLauncherVersion()
				};

				// Check for updates
				this.updater = new Updater(
				appVersions: appVersions,
                updateCompletedCallback: this.LauncherForm_DownloadCompletedCallback,
                updateProgressCallback: this.LauncherForm_DownloadProgressCallback);

				Task<VersionInfo> gameVersionInfo = this.GetVersionInfo(ApplicationNames.Giants);
				Task<VersionInfo> launcherVersionInfo = this.GetVersionInfo(ApplicationNames.GiantsLauncher);

				await Task.WhenAll(gameVersionInfo, launcherVersionInfo);

				await this.updater.UpdateApplication(ApplicationType.Game, gameVersionInfo.Result);
				await this.updater.UpdateApplication(ApplicationType.Launcher, launcherVersionInfo.Result);
			}
        }

		private async Task<VersionInfo> GetVersionInfo(string appName)
        {
			VersionInfo versionInfo;
			try
			{
				versionInfo = await this.httpClient.GetVersionInfoAsync(appName);
				return versionInfo;
			}
			catch (ApiException ex)
			{
#if DEBUG
				MessageBox.Show($"Exception retrieving version information: {ex.StatusCode}");
#endif
				return null;
			}
			catch (Exception ex)
			{
#if DEBUG
				MessageBox.Show($"Exception retrieving version information: {ex.Message}");
#endif
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
            this.txtProgress.Text = string.Format(Resources.DownloadProgress, e.ProgressPercentage, (info.FileSize / 1024) / 1024);
		}
	}
}
