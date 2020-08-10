namespace Giants.Services
{
    using System;
    using System.Threading.Tasks;

    public class FileUpdaterStore : IUpdaterStore
    {
        public Task<VersionInfo> GetVersionInfo(string gameName)
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
