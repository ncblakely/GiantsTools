namespace Giants.Launcher
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    public static class VersionHelper
    {
        public static Version GetGameVersion(string gamePath)
        {
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(gamePath);
                return new Version(fvi.FileVersion.Replace(',', '.'));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Version GetLauncherVersion()
        {
            return new Version(Application.ProductVersion);
        }
    }
}
