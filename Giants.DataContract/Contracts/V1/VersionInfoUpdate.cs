using Giants.DataContract.V1;

namespace Giants.DataContract.Contracts.V1
{
    public record VersionInfoUpdate(string AppName, AppVersion AppVersion, string FileName, string BranchName, bool ForceUpdate)
    {
        public string InstallerSha256 { get; init; }

        public string InstallerSignature { get; init; }
    }
}
