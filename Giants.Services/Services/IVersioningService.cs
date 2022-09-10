namespace Giants.Services
{
    using Giants.DataContract.V1;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IVersioningService
    {
        Task<IEnumerable<string>> GetBranches(string appName);

        Task<VersionInfo> GetVersionInfo(string appName, string branchName);

        Task UpdateVersionInfo(string appName, AppVersion appVersion, string fileName, string branchName, bool force);
    }
}
