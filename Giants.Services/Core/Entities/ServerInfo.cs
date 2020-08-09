namespace Giants.Services
{
    using System;
    using Giants.Services.Core.Entities;

    public class ServerInfo : DataContract.ServerInfo, IIdentifiable
    {
        public string id => HostIpAddress;

        public string HostIpAddress { get; set; }

        public DateTime LastHeartbeat { get; set; }

        public string DocumentType => nameof(ServerInfo);

        public override bool Equals(object obj)
        {
            return obj is ServerInfo info &&
                   base.Equals(obj) &&
                   HostIpAddress == info.HostIpAddress &&
                   DocumentType == info.DocumentType;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(id);
            hash.Add(HostIpAddress);
            hash.Add(DocumentType);
            return hash.ToHashCode();
        }
    }
}
