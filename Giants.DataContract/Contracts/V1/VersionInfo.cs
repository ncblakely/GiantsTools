namespace Giants.DataContract.V1
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class VersionInfo
    {
        [Required]
        public string AppName { get; set; }

        [Required]
        public AppVersion Version { get; set; }

        [Required]
        public Uri InstallerUri { get; set; }
    }
}
