namespace Giants.Services
{
    using Giants.DataContract.V1;
    using System.Threading.Tasks;

    public interface IVersioningService
    {
        Task<VersionInfo> GetVersionInfo(string appName);

        Task UpdateVersionInfo(string appName, AppVersion appVersion, string fileName);
    }
}
