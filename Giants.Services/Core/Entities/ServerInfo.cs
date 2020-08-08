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
    }
}
