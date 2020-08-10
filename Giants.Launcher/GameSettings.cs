using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Giants.Launcher
{
    static class GameSettings
	{
		// Constants
		private const string REGISTRY_KEY = @"HKEY_CURRENT_USER\Software\PlanetMoon\Giants";
		private const int OPTIONS_VERSION = 3;

		private static readonly Dictionary<string, object> Settings = new Dictionary<string, object>();

		// List of renderers compatible with the user's system.
		static public List<RendererInterop.Capabilities> CompatibleRenderers;

		public static object Get(string settingName)
		{
			if (Settings.ContainsKey(settingName))
				return Settings[settingName];
			else
				return 0;

		}

		public static void Modify(string settingName, object settingValue)
		{
			Settings[settingName] = settingValue;
		}

		public static void SetDefaults(string gamePath)
		{
			// Set default settings:
			Settings["Renderer"] = "gg_dx7r.dll";
			Settings["Antialiasing"] = 0;
			Settings["AnisotropicFiltering"] = 0;
			Settings["VideoWidth"] = 640;
			Settings["VideoHeight"] = 480;
			Settings["VideoDepth"] = 32;
			Settings["Windowed"] = 0;
			Settings["BorderlessWindow"] = 0;
			Settings["VerticalSync"] = 1;
			Settings["TripleBuffering"] = 1;
			Settings["NoAutoUpdate"] = 0;

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
				Settings["Renderer"] = Path.GetFileName(CompatibleRenderers.Max().FilePath);

			// Set the current desktop resolution, leaving bit depth at the default 32:
			Settings["VideoWidth"] = Screen.PrimaryScreen.Bounds.Width;
			Settings["VideoHeight"] = Screen.PrimaryScreen.Bounds.Height;
		}

		public static void Load(string gamePath)
		{
			SetDefaults(gamePath);

			if ((int)Registry.GetValue(REGISTRY_KEY, "GameOptionsVersion", 0) == OPTIONS_VERSION)
			{
				try
				{
					Settings["Renderer"] = RegistryExtensions.GetValue(REGISTRY_KEY, "Renderer", Settings["Renderer"], typeof(string));
					//System.Diagnostics.Debug.Assert(_Settings["Renderer"] is string);
					Settings["Antialiasing"] = RegistryExtensions.GetValue(REGISTRY_KEY, "Antialiasing", Settings["Antialiasing"], typeof(int));
					Settings["AnisotropicFiltering"] = RegistryExtensions.GetValue(REGISTRY_KEY, "AnisotropicFiltering", Settings["AnisotropicFiltering"], typeof(int));
					Settings["VideoWidth"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VideoWidth", Settings["VideoWidth"], typeof(int));
					Settings["VideoHeight"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VideoHeight", Settings["VideoHeight"], typeof(int));
					Settings["VideoDepth"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VideoDepth", Settings["VideoDepth"], typeof(int));
					Settings["Windowed"] = RegistryExtensions.GetValue(REGISTRY_KEY, "Windowed", Settings["Windowed"], typeof(int));
					Settings["BorderlessWindow"] = RegistryExtensions.GetValue(REGISTRY_KEY, "BorderlessWindow", Settings["BorderlessWindow"], typeof(int));
					Settings["VerticalSync"] = RegistryExtensions.GetValue(REGISTRY_KEY, "VerticalSync", Settings["VerticalSync"], typeof(int));
					Settings["TripleBuffering"] = RegistryExtensions.GetValue(REGISTRY_KEY, "TripleBuffering", Settings["TripleBuffering"], typeof(int));
					Settings["NoAutoUpdate"] = RegistryExtensions.GetValue(REGISTRY_KEY, "NoAutoUpdate", Settings["NoAutoUpdate"], typeof(int));
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
				Registry.SetValue(REGISTRY_KEY, "Renderer", Settings["Renderer"], RegistryValueKind.String);
				Registry.SetValue(REGISTRY_KEY, "Antialiasing", Settings["Antialiasing"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "AnisotropicFiltering", Settings["AnisotropicFiltering"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VideoWidth", Settings["VideoWidth"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VideoHeight", Settings["VideoHeight"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VideoDepth", Settings["VideoDepth"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "Windowed", Settings["Windowed"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "BorderlessWindow", Settings["BorderlessWindow"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "VerticalSync", Settings["VerticalSync"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "TripleBuffering", Settings["TripleBuffering"], RegistryValueKind.DWord);
				Registry.SetValue(REGISTRY_KEY, "NoAutoUpdate", Settings["NoAutoUpdate"], RegistryValueKind.DWord);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Could not save game settings to registry!\n\nReason: {0}", ex.Message));
			}
		}
	}
}
