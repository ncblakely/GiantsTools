using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Giants.Launcher
{
    public partial class OptionsForm : Form
	{
        private readonly string gamePath = null;

		public OptionsForm(string title, string gamePath)
		{
            this.InitializeComponent();

			this.Text = title;
			this.gamePath = gamePath;
		}

		private void OptionsForm_Load(object sender, EventArgs e)
		{
            this.PopulateResolution();
            this.SetOptions();
		}

		private void SetOptions()
		{
            this.cmbRenderer.Items.Clear();
            this.cmbRenderer.Items.AddRange(
				GameSettings.CompatibleRenderers
					.Disambiguate()
					.ToList()
					.ToArray());

			RendererInfo renderer = GameSettings.CompatibleRenderers.Find(
				r => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(r.FilePath), GameSettings.Get<string>(SettingKeys.Renderer)) == 0);

			if (renderer != null)
			{
                this.cmbRenderer.SelectedItem = renderer;
			}
			else
			{
				renderer = GameSettings.CompatibleRenderers.Find(r => r.Name == "DirectX 7");
                this.cmbRenderer.SelectedItem = renderer;
			}

			var resolutions = (List<ScreenResolution>)this.cmbResolution.DataSource;
            this.cmbResolution.SelectedItem = resolutions.Find(r => r.Width == GameSettings.Get<int>(SettingKeys.VideoWidth) && r.Height == GameSettings.Get<int>(SettingKeys.VideoHeight));
			if (this.cmbResolution.SelectedItem == null)
                this.cmbResolution.SelectedIndex = 0;

			var AntialiasingOptions = (List<KeyValuePair<string, int>>)this.cmbAntialiasing.DataSource;
            this.cmbAntialiasing.SelectedItem = AntialiasingOptions.Find(o => o.Value == GameSettings.Get<int>(SettingKeys.Antialiasing));
			if (this.cmbAntialiasing.SelectedItem == null)
                this.cmbAntialiasing.SelectedIndex = 0;

			var AnisotropyOptions = (List<KeyValuePair<string, int>>)this.cmbAnisotropy.DataSource;
            this.cmbAnisotropy.SelectedItem = AnisotropyOptions.Find(o => o.Value == GameSettings.Get<int>(SettingKeys.AnisotropicFiltering));
			if (this.cmbAnisotropy.SelectedItem == null)
                this.cmbAnisotropy.SelectedIndex = 0;

            this.chkUpdates.Checked = GameSettings.Get<int>(SettingKeys.NoAutoUpdate) != 1;
		}

		private void PopulateAntialiasing()
		{
			var antialiasingOptions = new List<KeyValuePair<string, int>>();
			antialiasingOptions.Add(new KeyValuePair<string, int>(Resources.OptionNone, 0));

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
            this.PopulateAntialiasing();
            this.PopulateAnisotropy();

			bool windowed = GameSettings.Get<int>(SettingKeys.Windowed) == 1;
			if (windowed)
			{
				bool borderless = GameSettings.Get<int>(SettingKeys.BorderlessWindow) == 1;
				if (borderless)
                    this.cmbMode.SelectedIndex = 2;
				else
                    this.cmbMode.SelectedIndex = 1;
			}
			else
                this.cmbMode.SelectedIndex = 0;

			var renderer = (RendererInfo)this.cmbRenderer.SelectedItem;

			if ((renderer.Flags & RendererInfo.RendererFlag.VSync) != RendererInfo.RendererFlag.VSync)
			{
                this.chkVSync.Checked = false;
                this.chkVSync.Enabled = false;
			}
			else
			{
                this.chkVSync.Checked = GameSettings.Get<int>(SettingKeys.VerticalSync) == 1;
                this.chkVSync.Enabled = true;
			}

			if ((renderer.Flags & RendererInfo.RendererFlag.TripleBuffer) != RendererInfo.RendererFlag.TripleBuffer)
			{
                this.chkTripleBuffering.Checked = false;
                this.chkTripleBuffering.Enabled = false;
			}
			else
			{
                this.chkTripleBuffering.Checked = GameSettings.Get<int>(SettingKeys.TripleBuffering) == 1;
                this.chkTripleBuffering.Enabled = true;
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			var renderer = this.cmbRenderer.SelectedItem as RendererInfo;
			if (renderer != null)
			{
				GameSettings.Modify(SettingKeys.Renderer, renderer.FileName);
			}

			var resolution = (ScreenResolution)this.cmbResolution.SelectedItem;
			if (resolution != null)
			{
				GameSettings.Modify(SettingKeys.VideoWidth, resolution.Width);
				GameSettings.Modify(SettingKeys.VideoHeight, resolution.Height);
			}

			GameSettings.Modify(SettingKeys.Antialiasing, this.cmbAntialiasing.SelectedValue);
			GameSettings.Modify(SettingKeys.AnisotropicFiltering, this.cmbAnisotropy.SelectedValue);
			bool windowed = (WindowType)this.cmbMode.SelectedIndex == WindowType.Windowed || (WindowType)this.cmbMode.SelectedIndex == WindowType.Borderless;
			GameSettings.Modify(SettingKeys.Windowed, windowed  == true ? 1 : 0);
			bool borderless = (WindowType)this.cmbMode.SelectedIndex == WindowType.Borderless;
			GameSettings.Modify(SettingKeys.BorderlessWindow, borderless == true ? 1 : 0);
			GameSettings.Modify(SettingKeys.VerticalSync, this.chkVSync.Checked == true ? 1 : 0);
			GameSettings.Modify(SettingKeys.TripleBuffering, this.chkTripleBuffering.Checked == true ? 1 : 0);
			GameSettings.Modify(SettingKeys.NoAutoUpdate, this.chkUpdates.Checked == false ? 1 : 0);

			GameSettings.Save();

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
