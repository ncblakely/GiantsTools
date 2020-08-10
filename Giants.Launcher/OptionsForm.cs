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
			InitializeComponent();

			this.Text = title;
			this.gamePath = gamePath;
		}

		private void OptionsForm_Load(object sender, EventArgs e)
		{
			PopulateResolution();
			SetOptions();
		}

		private void SetOptions()
		{
			cmbRenderer.Items.Clear();
			cmbRenderer.Items.AddRange(GameSettings.CompatibleRenderers.ToArray());

			RendererInterop.Capabilities renderer = GameSettings.CompatibleRenderers.Find(r => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(r.FilePath), GameSettings.Get("Renderer")) == 0);
			if (renderer != null)
				cmbRenderer.SelectedItem = renderer;
			else
			{
				renderer = GameSettings.CompatibleRenderers.Find(r => r.Name == "DirectX 7");
				cmbRenderer.SelectedItem = renderer;
			}

			var Resolutions = (List<ScreenResolution>)cmbResolution.DataSource;
			cmbResolution.SelectedItem = Resolutions.Find(r => r.Width == (int)GameSettings.Get("VideoWidth") && r.Height == (int)GameSettings.Get("VideoHeight"));
			if (cmbResolution.SelectedItem == null)
				cmbResolution.SelectedIndex = 0;

			var AntialiasingOptions = (List<KeyValuePair<string, int>>)cmbAntialiasing.DataSource;
			cmbAntialiasing.SelectedItem = AntialiasingOptions.Find(o => o.Value == (int)GameSettings.Get("Antialiasing"));
			if (cmbAntialiasing.SelectedItem == null)
				cmbAntialiasing.SelectedIndex = 0;

			var AnisotropyOptions = (List<KeyValuePair<string, int>>)cmbAnisotropy.DataSource;
			cmbAnisotropy.SelectedItem = AnisotropyOptions.Find(o => o.Value == (int)GameSettings.Get("AnisotropicFiltering"));
			if (cmbAnisotropy.SelectedItem == null)
				cmbAnisotropy.SelectedIndex = 0;

			chkUpdates.Checked = ((int)GameSettings.Get("NoAutoUpdate") == 1 ? false : true);
		}

		private void PopulateAntialiasing()
		{
			List<KeyValuePair<string, int>> AntialiasingOptions = new List<KeyValuePair<string, int>>();

			AntialiasingOptions.Add(new KeyValuePair<string, int>("None (Best performance)", 0));

			var renderer = (RendererInterop.Capabilities)cmbRenderer.SelectedItem;
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
			if (cmbAntialiasing.SelectedValue != null)
			{
				currentValue = (int)cmbAntialiasing.SelectedValue;
			}

			cmbAntialiasing.DataSource = AntialiasingOptions;
			cmbAntialiasing.DisplayMember = "Key";
			cmbAntialiasing.ValueMember = "Value";

			if (currentValue != null)
				cmbAntialiasing.SelectedValue = currentValue;
			
			if (cmbAntialiasing.SelectedValue == null)
				cmbAntialiasing.SelectedIndex = 0;
		}

		private bool IsPowerOfTwo(int x)
		{
			return (x != 0) && ((x & (x - 1)) == 0);
		}

		private void PopulateAnisotropy()
		{
			List<KeyValuePair<string, int>> AnisotropyOptions = new List<KeyValuePair<string, int>>();

			AnisotropyOptions.Add(new KeyValuePair<string, int>("None (Best performance)", 0));

			var renderer = (RendererInterop.Capabilities)cmbRenderer.SelectedItem;
			if (renderer != null)
			{
				for (int i = 2; i <= renderer.MaxAnisotropy; i++)
				{
					if (!IsPowerOfTwo(i)) continue;

					AnisotropyOptions.Add(new KeyValuePair<string,int>(String.Format("{0} Samples", i), i));
				}
			}

			// Try to keep current selection when repopulating
			int? currentValue = null;
			if (cmbAnisotropy.SelectedValue != null)
			{
				currentValue = (int)cmbAnisotropy.SelectedValue;
			}

			cmbAnisotropy.DataSource = AnisotropyOptions;
			cmbAnisotropy.DisplayMember = "Key";
			cmbAnisotropy.ValueMember = "Value";

			if (currentValue != null)
				cmbAnisotropy.SelectedValue = currentValue;

			if (cmbAnisotropy.SelectedValue == null)
				cmbAnisotropy.SelectedIndex = 0;
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
			cmbResolution.DataSource = resolutions;
		}

		private void cmbRenderer_SelectedIndexChanged(object sender, EventArgs e)
		{
			PopulateAntialiasing();
			PopulateAnisotropy();

			bool windowed = ((int)GameSettings.Get("Windowed") == 1 ? true : false);
			if (windowed)
			{
				bool borderless = (int)GameSettings.Get("BorderlessWindow") == 1 ? true : false;
				if (borderless)
					cmbMode.SelectedIndex = 2;
				else
					cmbMode.SelectedIndex = 1;
			}
			else
				cmbMode.SelectedIndex = 0;

			var renderer = (RendererInterop.Capabilities)cmbRenderer.SelectedItem;

			if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.VSync) != RendererInterop.Capabilities.RendererFlag.VSync)
			{
				chkVSync.Checked = false;
				chkVSync.Enabled = false;
			}
			else
			{
				chkVSync.Checked = ((int)GameSettings.Get("VerticalSync") == 1 ? true : false);
				chkVSync.Enabled = true;
			}

			if ((renderer.Flags & RendererInterop.Capabilities.RendererFlag.TripleBuffer) != RendererInterop.Capabilities.RendererFlag.TripleBuffer)
			{
				chkTripleBuffering.Checked = false;
				chkTripleBuffering.Enabled = false;
			}
			else
			{
				chkTripleBuffering.Checked = ((int)GameSettings.Get("TripleBuffering") == 1 ? true : false);
				chkTripleBuffering.Enabled = true;
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			var renderer = cmbRenderer.SelectedItem as RendererInterop.Capabilities;
			if (renderer != null)
			{
				GameSettings.Modify("Renderer", renderer.FileName);
			}

			var resolution = (ScreenResolution)cmbResolution.SelectedItem;
			if (resolution != null)
			{
				GameSettings.Modify("VideoWidth", resolution.Width);
				GameSettings.Modify("VideoHeight", resolution.Height);
			}

			GameSettings.Modify("Antialiasing", cmbAntialiasing.SelectedValue);
			GameSettings.Modify("AnisotropicFiltering", cmbAnisotropy.SelectedValue);
			bool windowed = ((WindowType)cmbMode.SelectedIndex == WindowType.Windowed || (WindowType)cmbMode.SelectedIndex == WindowType.Borderless);
			GameSettings.Modify("Windowed", (windowed  == true ? 1 : 0));
			bool borderless = (WindowType)cmbMode.SelectedIndex == WindowType.Borderless;
			GameSettings.Modify("BorderlessWindow", borderless == true ? 1 : 0);
			GameSettings.Modify("VerticalSync", (chkVSync.Checked == true ? 1 : 0));
			GameSettings.Modify("TripleBuffering", (chkTripleBuffering.Checked == true ? 1 : 0));
			GameSettings.Modify("NoAutoUpdate", (chkUpdates.Checked == false ? 1 : 0));

			GameSettings.Save();

			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
			GameSettings.Load(gamePath);
		}

		private void btnResetDefaults_Click(object sender, EventArgs e)
		{
			GameSettings.SetDefaults(gamePath);
			SetOptions();
		}
	}
}
