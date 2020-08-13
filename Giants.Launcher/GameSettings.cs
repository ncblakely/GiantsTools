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
        private const string RegistryKey = @"HKEY_CURRENT_USER\Software\PlanetMoon\Giants";
        private const int OptionsVersion = 3;

        private static readonly Dictionary<string, object> Settings = new Dictionary<string, object>();

        // List of renderers compatible with the user's system.
        public static List<RendererInterop.Capabilities> CompatibleRenderers;

        public static T Get<T>(string settingName)
        {
            return (T)Get(settingName);
        }

        public static object Get(string settingName)
        {
            if (Settings.ContainsKey(settingName))
            {
                return Settings[settingName];
            }
            else
            { 
                return 0;
            }
        }

        public static void Modify(string settingName, object settingValue)
        {
            Settings[settingName] = settingValue;
        }

        public static void SetDefaults(string gamePath)
        {
            // Set default settings:
            Settings[SettingKeys.Renderer] = "gg_dx7r.dll";
            Settings[SettingKeys.Antialiasing] = 0;
            Settings[SettingKeys.AnisotropicFiltering] = 0;
            Settings[SettingKeys.VideoDepth] = 32;
            Settings[SettingKeys.Windowed] = 0;
            Settings[SettingKeys.BorderlessWindow] = 0;
            Settings[SettingKeys.VerticalSync] = 1;
            Settings[SettingKeys.TripleBuffering] = 1;
            Settings[SettingKeys.NoAutoUpdate] = 0;

            // Get a list of renderers compatible with the user's system
            if (CompatibleRenderers == null)
            {
                CompatibleRenderers = RendererInterop.GetCompatibleRenderers(gamePath);
                if (CompatibleRenderers.Count == 0)
                {
                    MessageBox.Show(
                        text: Resources.ErrorNoRenderers,
                        caption: Resources.Error,
                        buttons: MessageBoxButtons.OK,
                        icon: MessageBoxIcon.Error);
                }
            }

            // Select the highest priority renderer
            if (CompatibleRenderers.Any())
            {
                Settings[SettingKeys.Renderer] = Path.GetFileName(CompatibleRenderers.Max().FilePath);
            }

            // Set the current desktop resolution, leaving bit depth at the default 32:
            Settings[SettingKeys.VideoWidth] = Screen.PrimaryScreen.Bounds.Width;
            Settings[SettingKeys.VideoHeight] = Screen.PrimaryScreen.Bounds.Height;
        }

        public static void Load(string gamePath)
        {
            SetDefaults(gamePath);

            if ((int)Registry.GetValue(RegistryKey, SettingKeys.GameOptionsVersion, 0) == OptionsVersion)
            {
                try
                {
                    Settings[SettingKeys.Renderer] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.Renderer, SettingKeys.Renderer, typeof(string));
                    Settings[SettingKeys.Antialiasing] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.Antialiasing, Settings[SettingKeys.Antialiasing], typeof(int));
                    Settings[SettingKeys.AnisotropicFiltering] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.AnisotropicFiltering, Settings[SettingKeys.AnisotropicFiltering], typeof(int));
                    Settings[SettingKeys.VideoWidth] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.VideoWidth, Settings[SettingKeys.VideoWidth], typeof(int));
                    Settings[SettingKeys.VideoHeight] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.VideoHeight, Settings[SettingKeys.VideoHeight], typeof(int));
                    Settings[SettingKeys.VideoDepth] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.VideoDepth, Settings[SettingKeys.VideoDepth], typeof(int));
                    Settings[SettingKeys.Windowed] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.Windowed, Settings[SettingKeys.Windowed], typeof(int));
                    Settings[SettingKeys.BorderlessWindow] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.BorderlessWindow, Settings[SettingKeys.BorderlessWindow], typeof(int));
                    Settings[SettingKeys.VerticalSync] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.VerticalSync, Settings[SettingKeys.VerticalSync], typeof(int));
                    Settings[SettingKeys.TripleBuffering] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.TripleBuffering, Settings[SettingKeys.TripleBuffering], typeof(int));
                    Settings[SettingKeys.NoAutoUpdate] = RegistryExtensions.GetValue(RegistryKey, SettingKeys.NoAutoUpdate, Settings[SettingKeys.NoAutoUpdate], typeof(int));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        text: string.Format(Resources.ErrorSettingsLoad, ex.Message),
                        caption: Resources.Error,
                        buttons: MessageBoxButtons.OK,
                        icon: MessageBoxIcon.Error);
                }
            }
        }

        public static void Save()
        {
            try
            {
                Registry.SetValue(RegistryKey, SettingKeys.GameOptionsVersion, OptionsVersion, RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.Renderer, Settings[SettingKeys.Renderer], RegistryValueKind.String);
                Registry.SetValue(RegistryKey, SettingKeys.Antialiasing, Settings[SettingKeys.Antialiasing], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.AnisotropicFiltering, Settings[SettingKeys.AnisotropicFiltering], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.VideoWidth, Settings[SettingKeys.VideoWidth], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.VideoHeight, Settings[SettingKeys.VideoHeight], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.VideoDepth, Settings[SettingKeys.VideoDepth], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.Windowed, Settings[SettingKeys.Windowed], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.BorderlessWindow, Settings[SettingKeys.BorderlessWindow], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.VerticalSync, Settings[SettingKeys.VerticalSync], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.TripleBuffering, Settings[SettingKeys.TripleBuffering], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, SettingKeys.NoAutoUpdate, Settings[SettingKeys.NoAutoUpdate], RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    text: string.Format(Resources.ErrorSettingsSave, ex.Message),
                    caption: Resources.Error,
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Error);
            }
        }
    }
}
