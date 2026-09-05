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

        // Relative path to the installer file. This is preferred over InstallerUri by new clients.
        public string InstallerPath { get; set; }

        // SHA-256 digest of the installer, encoded as lowercase hexadecimal.
        public string InstallerSha256 { get; set; }

        // RS256 signature over the canonical update manifest, encoded as base64url.
        public string InstallerSignature { get; set; }

        [Required]
        public string BranchName { get; set;}
    }
}
