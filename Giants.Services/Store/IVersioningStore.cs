namespace Giants.Services
{
    using System.Threading.Tasks;

    public interface IVersioningStore
    {
        Task<VersionInfo> GetVersionInfo(string appName);

        Task UpdateVersionInfo(VersionInfo versionInfo);

        Task Initialize();
    }
}
