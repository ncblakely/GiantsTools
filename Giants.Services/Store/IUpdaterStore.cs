namespace Giants.Services
{
    using System.Threading.Tasks;

    public interface IUpdaterStore
    {
        Task<VersionInfo> GetVersionInfo(string appName);

        Task Initialize();
    }
}
