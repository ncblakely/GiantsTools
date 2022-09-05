namespace Giants.Services
{
    using System;
    using System.Threading.Tasks;

    public class FileVersioningStore : IVersioningStore
    {
        public Task<VersionInfo> GetVersionInfo(string appName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateVersionInfo(VersionInfo versionInfo)
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
