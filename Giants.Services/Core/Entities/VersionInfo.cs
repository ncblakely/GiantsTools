using System;

namespace Giants.Services
{
    public class VersionInfo : DataContract.V1.VersionInfo, IIdentifiable
    {
        public string id => GenerateId(this.AppName);

        public string DocumentType => nameof(VersionInfo);

        public static string GenerateId(string gameName) => $"{nameof(VersionInfo)}-{gameName}";
    }
}
