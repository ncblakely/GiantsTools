using Giants.WebApi.Clients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giants.Launcher
{
    public partial class OptionsForm : Form
	{
        private readonly string gamePath = null;
		private readonly string appName;
		private readonly Config config;
		private readonly string currentBranchName;
		private readonly bool enableBranchSelection;
		private readonly BranchesClient branchesClient;

		public string SelectedBranch { get; set; }

		public OptionsForm(
			string title, 
			string gamePath, 
			string appName,
			Config config,
			string currentBranchName,
			BranchesClient branchesClient)
		{
            this.InitializeComponent();

			this.Text = title;
			this.gamePath = gamePath;
			this.appName = appName;
			this.config = config;
			this.currentBranchName = currentBranchName;
			this.branchesClient = branchesClient;

            this.config.TryGetBool(ConfigSections.Update, ConfigKeys.EnableBranchSelection, defaultValue: false, out bool enableBranchSelection);
            this.enableBranchSelection = enableBranchSelection;

            // Must come first as other options depend on it
            this.PopulateRenderers();
            this.SetRenderer();

            this.PopulateResolution();
            this.PopulateAnisotropy();
            this.PopulateAntialiasing();
			this.PopulateMode();

            this.SetOptions();
        }

		private async void OptionsForm_Load(object sender, EventArgs e)
		{
            await this.PopulateBranches();
        }

		private async Task PopulateBranches()
		{
			if (this.enableBranchSelection)
			{
				try
				{
					var branches = await this.branchesClient.GetBranchesAsync(this.appName);

                    cmbBranch.Items.AddRange(branches.ToArray());

					cmbBranch.SelectedItem = this.currentBranchName;
                    cmbBranch.Visible = true;
                }
				catch (Exception e)
				{
					MessageBox.Show($"Unhandled exception retrieving branch information from the server: {e.Message}");
				}
			}
		}

		private void PopulateRenderers()
        {
			this.cmbRenderer.Items.Clear();
			this.cmbRenderer.Items.AddRange(
				GameSettings.CompatibleRenderers
					.Disambiguate()
					.ToList()
					.ToArray());
		}

		private void SetRenderer()
        {
			string selectedRenderer = GameSettings.Get<string>(RegistryKeys.Renderer);
			RendererInfo renderer = 
				GameSettings.CompatibleRenderers.Find(
					r => Path.GetFileName(r.FilePath).Equals(selectedRenderer, StringComparison.OrdinalIgnoreCase));

			if (renderer != null)
			{
				this.cmbRenderer.SelectedItem = renderer;
			}
			else
			{
				renderer = GameSettings.CompatibleRenderers.Find(r => r.FileName == "gg_dx9r.dll");
				this.cmbRenderer.SelectedItem = renderer;
			}
		}

		private void SetOptions()
		{
			SetRenderer();

			var resolutions = (List<ScreenResolution>)this.cmbResolution.DataSource;
            this.cmbResolution.SelectedItem = resolutions.Find(r => r.Width == GameSettings.Get<int>(RegistryKeys.VideoWidth) && r.Height == GameSettings.Get<int>(RegistryKeys.VideoHeight));
			if (this.cmbResolution.SelectedItem == null)
                this.cmbResolution.SelectedIndex = 0;

			var antialiasingOptions = (List<KeyValuePair<string, int>>)this.cmbAntialiasing.DataSource;
            this.cmbAntialiasing.SelectedItem = antialiasingOptions.Find(o => o.Value == GameSettings.Get<int>(RegistryKeys.Antialiasing));
			if (this.cmbAntialiasing.SelectedItem == null)
                this.cmbAntialiasing.SelectedIndex = 0;

			var anisotropyOptions = (List<KeyValuePair<string, int>>)this.cmbAnisotropy.DataSource;
            this.cmbAnisotropy.SelectedItem = anisotropyOptions.Find(o => o.Value == GameSettings.Get<int>(RegistryKeys.AnisotropicFiltering));
			if (this.cmbAnisotropy.SelectedItem == null)
                this.cmbAnisotropy.SelectedIndex = 0;

            this.chkUpdates.Checked = GameSettings.Get<int>(RegistryKeys.NoAutoUpdate) != 1;

			if (this.enableBranchSelection)
			{
				this.cmbBranch.SelectedItem = this.currentBranchName;
			}
		}

		private void PopulateAntialiasing()
		{
			var antialiasingOptions = new List<KeyValuePair<string, int>>
			{
				new KeyValuePair<string, int>(Resources.OptionNone, 0)
			};

			var renderer = (RendererInfo)this.cmbRenderer.SelectedItem;
			if (renderer != null)
			{
				if (renderer.Flags.HasFlag(RendererInfo.RendererFlag.MSAA2x))
				{
					antialiasingOptions.Add(new KeyValuePair<string, int>(string.Format(Resources.OptionSamples, 2), 2));
				}
				if (renderer.Flags.HasFlag(RendererInfo.RendererFlag.MSAA4x))
				{
					antialiasingOptions.Add(new KeyValuePair<string, int>(string.Format(Resources.OptionSamples, 4), 4));
				}
				if (renderer.Flags.HasFlag(RendererInfo.RendererFlag.MSAA8x))
				{
					antialiasingOptions.Add(new KeyValuePair<string, int>(string.Format(Resources.OptionSamples, 8), 8));
				}
				if (renderer.Flags.HasFlag(RendererInfo.RendererFlag.MSAA16x))
				{
					antialiasingOptions.Add(new KeyValuePair<string, int>(string.Format(Resources.OptionSamples, 16), 16));
				}
			}

			// Try to keep current selection when repopulating
			int? currentValue = null;
			if (this.cmbAntialiasing.SelectedValue != null)
			{
				currentValue = (int)this.cmbAntialiasing.SelectedValue;
			}

            this.cmbAntialiasing.DataSource = antialiasingOptions;
            this.cmbAntialiasing.DisplayMember = "Key";
            this.cmbAntialiasing.ValueMember = "Value";

			if (currentValue != null)
                this.cmbAntialiasing.SelectedValue = currentValue;
			
			if (this.cmbAntialiasing.SelectedValue == null)
                this.cmbAntialiasing.SelectedIndex = 0;
		}

        private void PopulateMode()
        {
            var modeOptions = new List<KeyValuePair<string, int>>();

            var renderer = (RendererInfo)this.cmbRenderer.SelectedItem;
            if (renderer != null && renderer.Flags.HasFlag(RendererInfo.RendererFlag.Fullscreen))
                modeOptions.Add(new KeyValuePair<string, int>(Resources.Fullscreen, 0));

                modeOptions.Add(new KeyValuePair<string, int>(Resources.Windowed, 1));

            // Try to keep current selection when repopulating
            int? currentValue = null;
            if (this.cmbMode.SelectedValue != null)
            {
                currentValue = (int)this.cmbMode.SelectedValue;
            }

            this.cmbMode.DataSource = modeOptions;
            this.cmbMode.DisplayMember = "Key";
            this.cmbMode.ValueMember = "Value";

            if (currentValue != null)
                this.cmbMode.SelectedValue = currentValue;

            if (this.cmbMode.SelectedValue == null)
                this.cmbMode.SelectedIndex = 0;
        }

        private bool IsPowerOfTwo(int x)
		{
			return (x != 0) && ((x & (x - 1)) == 0);
		}

		private void PopulateAnisotropy()
		{
			var anisotropyOptions = new List<KeyValuePair<string, int>>();
			anisotropyOptions.Add(new KeyValuePair<string, int>(Resources.OptionNone, 0));

			var renderer = (RendererInfo)this.cmbRenderer.SelectedItem;
			if (renderer != null)
			{
				for (int i = 2; i <= renderer.MaxAnisotropy; i++)
				{
					if (!this.IsPowerOfTwo(i)) continue;

					anisotropyOptions.Add(new KeyValuePair<string,int>(string.Format(Resources.OptionSamples, i), i));
				}
			}

			// Try to keep current selection when repopulating
			int? currentValue = null;
			if (this.cmbAnisotropy.SelectedValue != null)
			{
				currentValue = (int)this.cmbAnisotropy.SelectedValue;
			}

            this.cmbAnisotropy.DataSource = anisotropyOptions;
            this.cmbAnisotropy.DisplayMember = "Key";
            this.cmbAnisotropy.ValueMember = "Value";

			if (currentValue != null)
                this.cmbAnisotropy.SelectedValue = currentValue;

			if (this.cmbAnisotropy.SelectedValue == null)
                this.cmbAnisotropy.SelectedIndex = 0;
		}

		private void PopulateResolution()
		{
			List<ScreenResolution> resolutions = new List<ScreenResolution>();

            NativeMethods.DEVMODE devMode = new NativeMethods.DEVMODE();
            int i = 0;
			while (NativeMethods.EnumDisplaySettings(null, i, ref devMode))
			{
				if (devMode.dmBitsPerPel == 32 && devMode.dmPelsWidth >= 640 && devMode.dmPelsHeight >= 480)
				{
					if (resolutions.Find(r => r.Width == devMode.dmPelsWidth && r.Height == devMode.dmPelsHeight) == null)
						resolutions.Add(new ScreenResolution(devMode.dmPelsWidth, devMode.dmPelsHeight));
				}
				i++;
			}

			resolutions.Sort();
            this.cmbResolution.DataSource = resolutions;
		}

		private void cmbRenderer_SelectedIndexChanged(object sender, EventArgs e)
		{
			var renderer = (RendererInfo)this.cmbRenderer.SelectedItem;

			if ((renderer.Flags & RendererInfo.RendererFlag.VSync) != RendererInfo.RendererFlag.VSync)
			{
                this.chkVSync.Checked = false;
                this.chkVSync.Enabled = false;
			}
			else
			{
                this.chkVSync.Checked = GameSettings.Get<int>(RegistryKeys.VerticalSync) == 1;
                this.chkVSync.Enabled = true;
			}

			if ((renderer.Flags & RendererInfo.RendererFlag.TripleBuffer) != RendererInfo.RendererFlag.TripleBuffer)
			{
                this.chkTripleBuffering.Checked = false;
                this.chkTripleBuffering.Enabled = false;
			}
			else
			{
                this.chkTripleBuffering.Checked = GameSettings.Get<int>(RegistryKeys.TripleBuffering) == 1;
                this.chkTripleBuffering.Enabled = true;
			}

			this.PopulateAntialiasing();
			this.PopulateMode();
			this.PopulateAnisotropy();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			var renderer = this.cmbRenderer.SelectedItem as RendererInfo;
			if (renderer != null)
			{
				GameSettings.Modify(RegistryKeys.Renderer, renderer.FileName);
			}

			var resolution = (ScreenResolution)this.cmbResolution.SelectedItem;
			if (resolution != null)
			{
				GameSettings.Modify(RegistryKeys.VideoWidth, resolution.Width);
				GameSettings.Modify(RegistryKeys.VideoHeight, resolution.Height);
			}

			GameSettings.Modify(RegistryKeys.Antialiasing, this.cmbAntialiasing.SelectedValue);
			GameSettings.Modify(RegistryKeys.AnisotropicFiltering, this.cmbAnisotropy.SelectedValue);
            bool windowed = (WindowType)this.cmbMode.SelectedIndex == WindowType.Windowed;
            GameSettings.Modify(RegistryKeys.Windowed, windowed  == true ? 1 : 0);
			GameSettings.Modify(RegistryKeys.VerticalSync, this.chkVSync.Checked == true ? 1 : 0);
			GameSettings.Modify(RegistryKeys.TripleBuffering, this.chkTripleBuffering.Checked == true ? 1 : 0);
			GameSettings.Modify(RegistryKeys.NoAutoUpdate, this.chkUpdates.Checked == false ? 1 : 0);

			GameSettings.Save();

			if (this.enableBranchSelection)
			{
				string newBranch = this.cmbBranch.SelectedItem?.ToString();
				if (!string.IsNullOrEmpty(newBranch) && !newBranch.Equals(this.currentBranchName, StringComparison.OrdinalIgnoreCase))
				{
					this.SelectedBranch = newBranch;
				}
			}

			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
			GameSettings.Load(this.gamePath);
		}

		private void btnResetDefaults_Click(object sender, EventArgs e)
		{
			GameSettings.SetDefaults(this.gamePath);
            this.SetOptions();
		}
	}
}
