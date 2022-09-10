namespace Giants.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IVersioningStore
    {
        Task<IEnumerable<VersionInfo>> GetVersions();

        Task UpdateVersionInfo(VersionInfo versionInfo);
    }
}
