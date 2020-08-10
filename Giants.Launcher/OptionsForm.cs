using System;
using System.Collections.Generic;
using System.IO;
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
            this.cmbRenderer.Items.AddRange(GameSettings.CompatibleRenderers.ToArray());

			RendererInterop.Capabilities renderer = GameSettings.CompatibleRenderers.Find(r => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(r.FilePath), GameSettings.Get("Renderer")) == 0);
			if (renderer != null)
                this.cmbRenderer.SelectedItem = renderer;
			else
			{
				renderer = GameSettings.CompatibleRenderers.Find(r => r.Name == "DirectX 7");
                this.cmbRenderer.SelectedItem = renderer;
			}

			var Resolutions = (List<ScreenResolution>)this.cmbResolution.DataSource;
            this.cmbResolution.SelectedItem = Resolutions.Find(r => r.Width == (int)GameSettings.Get("VideoWidth") && r.Height == (int)GameSettings.Get("VideoHeight"));
			if (this.cmbResolution.SelectedItem == null)
                this.cmbResolution.SelectedIndex = 0;

			var AntialiasingOptions = (List<KeyValuePair<string, int>>)this.cmbAntialiasing.DataSource;
            this.cmbAntialiasing.SelectedItem = AntialiasingOptions.Find(o => o.Value == (int)GameSettings.Get("Antialiasing"));
			if (this.cmbAntialiasing.SelectedItem == null)
                this.cmbAntialiasing.SelectedIndex = 0;

			var AnisotropyOptions = (List<KeyValuePair<string, int>>)this.cmbAnisotropy.DataSource;
            this.cmbAnisotropy.SelectedItem = AnisotropyOptions.Find(o => o.Value == (int)GameSettings.Get("AnisotropicFiltering"));
			if (this.cmbAnisotropy.SelectedItem == null)
                this.cmbAnisotropy.SelectedIndex = 0;

            this.chkUpdates.Checked = ((int)GameSettings.Get("NoAutoUpdate") == 1 ? false : true);
		}

		private void PopulateAntialiasing()
		{
			List<KeyValuePair<string, int>> AntialiasingOptions = new List<KeyValuePair<string, int>>();

			AntialiasingOptions.Add(new KeyValuePair<string, int>("None (Best performance)", 0));

			var renderer = (RendererInterop.Capabilities)this.cmbRenderer.SelectedItem;
			if (renderer != null)
			{
				if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.MSAA2x) == RendererInterop.Capabilities.RendererFlag.MSAA2x)
					AntialiasingOptions.Add(new KeyValuePair<string, int>("2 Samples", 2));
				if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.MSAA4x) == RendererInterop.Capabilities.RendererFlag.MSAA4x)
					AntialiasingOptions.Add(new KeyValuePair<string, int>("4 Samples", 4));
				if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.MSAA8x) == RendererInterop.Capabilities.RendererFlag.MSAA8x)
					AntialiasingOptions.Add(new KeyValuePair<string, int>("8 Samples", 8));
				if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.MSAA16x) == RendererInterop.Capabilities.RendererFlag.MSAA16x)
					AntialiasingOptions.Add(new KeyValuePair<string, int>("16 Samples", 16));
			}

			// Try to keep current selection when repopulating
			int? currentValue = null;
			if (this.cmbAntialiasing.SelectedValue != null)
			{
				currentValue = (int)this.cmbAntialiasing.SelectedValue;
			}

            this.cmbAntialiasing.DataSource = AntialiasingOptions;
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
			List<KeyValuePair<string, int>> AnisotropyOptions = new List<KeyValuePair<string, int>>();

			AnisotropyOptions.Add(new KeyValuePair<string, int>("None (Best performance)", 0));

			var renderer = (RendererInterop.Capabilities)this.cmbRenderer.SelectedItem;
			if (renderer != null)
			{
				for (int i = 2; i <= renderer.MaxAnisotropy; i++)
				{
					if (!this.IsPowerOfTwo(i)) continue;

					AnisotropyOptions.Add(new KeyValuePair<string,int>(String.Format("{0} Samples", i), i));
				}
			}

			// Try to keep current selection when repopulating
			int? currentValue = null;
			if (this.cmbAnisotropy.SelectedValue != null)
			{
				currentValue = (int)this.cmbAnisotropy.SelectedValue;
			}

            this.cmbAnisotropy.DataSource = AnisotropyOptions;
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

			bool windowed = ((int)GameSettings.Get("Windowed") == 1 ? true : false);
			if (windowed)
			{
				bool borderless = (int)GameSettings.Get("BorderlessWindow") == 1 ? true : false;
				if (borderless)
                    this.cmbMode.SelectedIndex = 2;
				else
                    this.cmbMode.SelectedIndex = 1;
			}
			else
                this.cmbMode.SelectedIndex = 0;

			var renderer = (RendererInterop.Capabilities)this.cmbRenderer.SelectedItem;

			if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.VSync) != RendererInterop.Capabilities.RendererFlag.VSync)
			{
                this.chkVSync.Checked = false;
                this.chkVSync.Enabled = false;
			}
			else
			{
                this.chkVSync.Checked = ((int)GameSettings.Get("VerticalSync") == 1 ? true : false);
                this.chkVSync.Enabled = true;
			}

			if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.TripleBuffer) != RendererInterop.Capabilities.RendererFlag.TripleBuffer)
			{
                this.chkTripleBuffering.Checked = false;
                this.chkTripleBuffering.Enabled = false;
			}
			else
			{
                this.chkTripleBuffering.Checked = ((int)GameSettings.Get("TripleBuffering") == 1 ? true : false);
                this.chkTripleBuffering.Enabled = true;
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			var renderer = this.cmbRenderer.SelectedItem as RendererInterop.Capabilities;
			if (renderer != null)
			{
				GameSettings.Modify("Renderer", renderer.FileName);
			}

			var resolution = (ScreenResolution)this.cmbResolution.SelectedItem;
			if (resolution != null)
			{
				GameSettings.Modify("VideoWidth", resolution.Width);
				GameSettings.Modify("VideoHeight", resolution.Height);
			}

			GameSettings.Modify("Antialiasing", this.cmbAntialiasing.SelectedValue);
			GameSettings.Modify("AnisotropicFiltering", this.cmbAnisotropy.SelectedValue);
			bool windowed = ((WindowType)this.cmbMode.SelectedIndex == WindowType.Windowed || (WindowType)this.cmbMode.SelectedIndex == WindowType.Borderless);
			GameSettings.Modify("Windowed", (windowed  == true ? 1 : 0));
			bool borderless = (WindowType)this.cmbMode.SelectedIndex == WindowType.Borderless;
			GameSettings.Modify("BorderlessWindow", borderless == true ? 1 : 0);
			GameSettings.Modify("VerticalSync", (this.chkVSync.Checked == true ? 1 : 0));
			GameSettings.Modify("TripleBuffering", (this.chkTripleBuffering.Checked == true ? 1 : 0));
			GameSettings.Modify("NoAutoUpdate", (this.chkUpdates.Checked == false ? 1 : 0));

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
