namespace Giants.DataContract
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class VersionInfo
    {
        [Required]
        public string GameName { get; set; }

        [Required]
        public GiantsVersion GameVersion { get; set; }

        [Required]
        public GiantsVersion LauncherVersion { get; set; }

        [Required]
        public Uri PatchUri { get; set; }
    }
}
