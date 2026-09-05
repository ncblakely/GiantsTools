namespace Giants.DataContract.Contracts.V2
{
    using System.ComponentModel.DataAnnotations;

    public class VersionInfo
    {
        [Required]
        public string AppName { get; set; }

        [Required]
        public Giants.DataContract.V1.AppVersion Version { get; set; }

        // Relative path to the installer file
        [Required]
        public string InstallerPath { get; set; }

        [Required]
        public string BranchName { get; set; }
    }
}
