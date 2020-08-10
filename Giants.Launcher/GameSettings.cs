using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace Giants.Launcher
{
	static class GameSettings
	{
		// Constants
		private const string REGISTRY_KEY = @"HKEY_CURRENT_USER\Software\PlanetMoon\Giants";
		private const int OPTIONS_VERSION = 3;

		private static readonly Dictionary<string, object> _Settings = new Dictionary<string, object>();

		// List of renderers compatible with the user's system.
		static public List<RendererInterop.Capabilities> CompatibleRenderers;

		public static object Get(string settingName)
		{
			if (_Settings.ContainsKey(settingName))
				return _Settings[settingName];
			else
				return 0;

		}

		public static void Modify(string settingName, object settingValue)
		{
			_Settings[settingName] = settingValue;
		}

		public static void SetDefaults(string gamePath)
		{
			// Set default settings:
			_Settings["Renderer"] = "gg_dx7r.dll";
			_Settings["Antialiasing"] = 0;
			_Settings["AnisotropicFiltering"] = 0;
			_Settings["VideoWidth"] = 640;
			_Settings["VideoHeight"] = 480;
			_Settings["VideoDepth"] = 32;
			_Settings["Windowed"] = 0;
			_Settings["BorderlessWindow"] = 0;
			_Settings["VerticalSync"] = 1;
			_Settings["TripleBuffering"] = 1;
			_Settings["NoAutoUpdate"] = 0;

			// Get a list of renderers compatible with the user's system
			if (CompatibleRenderers == null)
			{
				CompatibleRenderers = RendererInterop.GetCompatibleRenderers(gamePath);
				if (CompatibleRenderers.Count == 0)
				{
					MessageBox.Show("Could not locate any renderers compatible with your system. The most compatible renderer has been selected, but you may experience difficulty running the game.",
						"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			// Select the highest priority renderer
			if (CompatibleRenderers.Count > 0)
				_Settings["Renderer"] = Path.GetFileName(CompatibleRenderers.Max().FilePath);

			// Set the current desktop resolution, leaving bit depth at the default 32:
			_Settings["VideoWidth"] = Screen.PrimaryScreen.Bounds.Width;
			_Settings["VideoHeight"] = Screen.PrimaryScreen.Bounds.Height;
		}

		public static void Load(string gamePath)
		{
			SetDefaults(gamePath);

			if ((int)Registry.GetValue(REGISTRY_KEY, "GameOptionsVersion", 0) == OPTIONS_VERSION)
			{
				try
				{
					_Settings["Renderer"] = RegistryExtensions.GetValue(REGISTRY_KEY, "Renderer", _Settings["Renderer"], typeof(string));
					//System.Diagnostics.Debug.Assert(_Settings["Renderer"] is string);
					_Settings["Antialiasing"] = RegistryExtensions.GetValue(REGISTRY_KEY, "Antialiasing", _Settings["Antialiasing"], typeof(int));
					_Settings["AnisotropicFiltering"] = RegistryExtensions.GetValue(REGISTRY_KEY, "AnisotropicFiltering", _Settings["AnisotropicFiltering"], typeof(int));
					_Settings["VideoWidth"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VideoWidth", _Settings["VideoWidth"], typeof(int));
					_Settings["VideoHeight"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VideoHeight", _Settings["VideoHeight"], typeof(int));
					_Settings["VideoDepth"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VideoDepth", _Settings["VideoDepth"], typeof(int));
					_Settings["Windowed"] = RegistryExtensions.GetValue(REGISTRY_KEY, "Windowed", _Settings["Windowed"], typeof(int));
					_Settings["BorderlessWindow"] = RegistryExtensions.GetValue(REGISTRY_KEY, "BorderlessWindow", _Settings["BorderlessWindow"], typeof(int));
					_Settings["VerticalSync"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VerticalSync", _Settings["VerticalSync"], typeof(int));
					_Settings["TripleBuffering"] = RegistryExtensions.GetValue(REGISTRY_KEY, "TripleBuffering", _Settings["TripleBuffering"], typeof(int));
					_Settings["NoAutoUpdate"] = RegistryExtensions.GetValue(REGISTRY_KEY, "NoAutoUpdate", _Settings["NoAutoUpdate"], typeof(int));
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("Could not read game settings from registry!\n\nReason: {0}", ex.Message));
				}
			}
		}

		public static void Save()
		{
			try
			{
				Registry.SetValue(REGISTRY_KEY, "GameOptionsVersion", 3, RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "Renderer", _Settings["Renderer"], RegistryValueKind.String);
				Registry.SetValue(REGISTRY_KEY, "Antialiasing", _Settings["Antialiasing"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "AnisotropicFiltering", _Settings["AnisotropicFiltering"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VideoWidth", _Settings["VideoWidth"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VideoHeight", _Settings["VideoHeight"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VideoDepth", _Settings["VideoDepth"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "Windowed", _Settings["Windowed"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "BorderlessWindow", _Settings["BorderlessWindow"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VerticalSync", _Settings["VerticalSync"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "TripleBuffering", _Settings["TripleBuffering"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "NoAutoUpdate", _Settings["NoAutoUpdate"], RegistryValueKind.DWord);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Could not save game settings to registry!\n\nReason: {0}", ex.Message));
			}
		}
	}
}
