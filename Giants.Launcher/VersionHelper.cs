namespace Giants.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms;

    public static class VersionHelper
    {
        public static bool TryGetGameVersion(string gamePath, out Version version, out string branch)
        {
            version = default;
            branch = string.Empty;

            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(gamePath);
                version = new Version(fvi.FileVersion.Replace(',', '.'));

                Dictionary<string, string> commentSettings = GetCommentSettings(fvi.Comments);
                if (commentSettings.ContainsKey("Branch"))
                {
                    branch = commentSettings["Branch"];
                }
                else
                {
                    branch = "Release";
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Version GetLauncherVersion()
        {
            return new Version(Application.ProductVersion);
        }

        private static Dictionary<string, string> GetCommentSettings(string comments)
        {
            Dictionary<string, string> commentSettingsDictionary = new Dictionary<string, string>();
            string[] commentSettings = comments.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (!commentSettings.Any())
            {
                return commentSettingsDictionary;
            }

            foreach (var keyValuePair in commentSettings)
            {
                string[] commentKeyValuePairs = keyValuePair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (commentKeyValuePairs.Length != 2)
                {
                    continue;
                }

                if (!commentSettingsDictionary.ContainsKey(commentKeyValuePairs[0]))
                {
                    commentSettingsDictionary.Add(commentKeyValuePairs[0], commentKeyValuePairs[1]);
                }
            }

            return commentSettingsDictionary;
        }
    }
}
