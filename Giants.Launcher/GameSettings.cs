using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Giants.Launcher
{
    public static class GameSettings
    {
        // Constants
        private const string RegistryKey = @"HKEY_CURRENT_USER\Software\PlanetMoon\Giants";
        private const int OptionsVersion = 3;

        private static readonly Dictionary<string, object> Settings = new Dictionary<string, object>();

        // List of renderers compatible with the user's system.
        public static List<RendererInfo> CompatibleRenderers { get; set; } = new List<RendererInfo>();
            
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
            Settings[RegistryKeys.Renderer] = "gg_dx7r.dll";
            Settings[RegistryKeys.Antialiasing] = 0;
            Settings[RegistryKeys.AnisotropicFiltering] = 0;
            Settings[RegistryKeys.VideoDepth] = 32;
            Settings[RegistryKeys.Windowed] = 0;
            Settings[RegistryKeys.BorderlessWindow] = 0;
            Settings[RegistryKeys.VerticalSync] = 1;
            Settings[RegistryKeys.TripleBuffering] = 1;
            Settings[RegistryKeys.NoAutoUpdate] = 0;

            // Get a list of renderers compatible with the user's system
            if (!CompatibleRenderers.Any())
            {
                CompatibleRenderers = RendererInterop.GetCompatibleRenderers(gamePath);
                if (!CompatibleRenderers.Any())
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
                Settings[RegistryKeys.Renderer] = Path.GetFileName(CompatibleRenderers.Max().FilePath);
            }

            // Set the current desktop resolution, leaving bit depth at the default 32:
            Settings[RegistryKeys.VideoWidth] = Screen.PrimaryScreen.Bounds.Width;
            Settings[RegistryKeys.VideoHeight] = Screen.PrimaryScreen.Bounds.Height;
        }

        public static void Load(string gamePath)
        {
            SetDefaults(gamePath);

            if ((int)Registry.GetValue(RegistryKey, RegistryKeys.GameOptionsVersion, 0) == OptionsVersion)
            {
                try
                {
                    Settings[RegistryKeys.Renderer] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.Renderer, RegistryKeys.Renderer, typeof(string));
                    Settings[RegistryKeys.Antialiasing] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.Antialiasing, Settings[RegistryKeys.Antialiasing], typeof(int));
                    Settings[RegistryKeys.AnisotropicFiltering] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.AnisotropicFiltering, Settings[RegistryKeys.AnisotropicFiltering], typeof(int));
                    Settings[RegistryKeys.VideoWidth] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.VideoWidth, Settings[RegistryKeys.VideoWidth], typeof(int));
                    Settings[RegistryKeys.VideoHeight] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.VideoHeight, Settings[RegistryKeys.VideoHeight], typeof(int));
                    Settings[RegistryKeys.VideoDepth] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.VideoDepth, Settings[RegistryKeys.VideoDepth], typeof(int));
                    Settings[RegistryKeys.Windowed] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.Windowed, Settings[RegistryKeys.Windowed], typeof(int));
                    Settings[RegistryKeys.BorderlessWindow] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.BorderlessWindow, Settings[RegistryKeys.BorderlessWindow], typeof(int));
                    Settings[RegistryKeys.VerticalSync] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.VerticalSync, Settings[RegistryKeys.VerticalSync], typeof(int));
                    Settings[RegistryKeys.TripleBuffering] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.TripleBuffering, Settings[RegistryKeys.TripleBuffering], typeof(int));
                    Settings[RegistryKeys.NoAutoUpdate] = RegistryExtensions.GetValue(RegistryKey, RegistryKeys.NoAutoUpdate, Settings[RegistryKeys.NoAutoUpdate], typeof(int));
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
                Registry.SetValue(RegistryKey, RegistryKeys.GameOptionsVersion, OptionsVersion, RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.Renderer, Settings[RegistryKeys.Renderer], RegistryValueKind.String);
                Registry.SetValue(RegistryKey, RegistryKeys.Antialiasing, Settings[RegistryKeys.Antialiasing], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.AnisotropicFiltering, Settings[RegistryKeys.AnisotropicFiltering], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.VideoWidth, Settings[RegistryKeys.VideoWidth], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.VideoHeight, Settings[RegistryKeys.VideoHeight], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.VideoDepth, Settings[RegistryKeys.VideoDepth], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.Windowed, Settings[RegistryKeys.Windowed], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.BorderlessWindow, Settings[RegistryKeys.BorderlessWindow], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.VerticalSync, Settings[RegistryKeys.VerticalSync], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.TripleBuffering, Settings[RegistryKeys.TripleBuffering], RegistryValueKind.DWord);
                Registry.SetValue(RegistryKey, RegistryKeys.NoAutoUpdate, Settings[RegistryKeys.NoAutoUpdate], RegistryValueKind.DWord);
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
