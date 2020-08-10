using System;

namespace Giants.Services
{
    public class VersionInfo : DataContract.VersionInfo, IIdentifiable
    {
        public string id => GenerateId(this.GameName);

        public string DocumentType => nameof(VersionInfo);

        public static string GenerateId(string gameName) => $"{nameof(VersionInfo)}-{gameName}";
    }
}
