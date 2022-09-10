using System;

namespace Giants.Services
{
    public class VersionInfo : DataContract.V1.VersionInfo, IIdentifiable
    {
        public string id => GenerateId(this.AppName, this.BranchName ?? BranchConstants.DefaultBranchName);

        public string DocumentType => nameof(VersionInfo);

        public static string GenerateId(string appName, string branchName) => $"{nameof(VersionInfo)}-{appName}-{branchName}";
    }
}
