namespace Giants.Services
{
    using System;

    public class ServerInfo : DataContract.ServerInfo
    {
        public string HostIpAddress { get; set; }

        public DateTime LastHeartbeat { get; set; }
    }
}
