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

        public async Task<VersionInfo> GetVersionInfo(string appName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName, nameof(appName));

            return await this.updaterStore.GetVersionInfo(appName);
        }
    }
}
