using System.Threading.Tasks;

namespace Giants.Services
{
    public class UpdaterService : IUpdaterService
    {
        private readonly IUpdaterStore updaterStore;

        public UpdaterService(IUpdaterStore updaterStore)
        {
            this.updaterStore = updaterStore;
        }

        public async Task<VersionInfo> GetVersionInfo(string gameName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(gameName, nameof(gameName));

            return await this.updaterStore.GetVersionInfo(gameName);
        }
    }
}
