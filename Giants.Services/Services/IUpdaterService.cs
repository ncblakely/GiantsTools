namespace Giants.Services
{
    using System.Threading.Tasks;

    public interface IUpdaterService
    {
        Task<VersionInfo> GetVersionInfo(string gameName);
    }
}
