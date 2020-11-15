﻿namespace Giants.Services
{
    using System;

    public class ServerInfo : DataContract.V1.ServerInfo, IIdentifiable
    {
        public string id => $"{this.HostIpAddress}-{this.GameName}-{this.Port}";

        public string DocumentType => nameof(ServerInfo);

        public string HostIpAddress { get; set; }

        public DateTime LastHeartbeat { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ServerInfo info &&
                   base.Equals(obj) &&
                   this.HostIpAddress == info.HostIpAddress &&
                   this.Port == info.Port &&
                   this.GameName == info.GameName &&
                   this.DocumentType == info.DocumentType;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(this.id);
            hash.Add(this.HostIpAddress);
            hash.Add(this.DocumentType);
            return hash.ToHashCode();
        }
    }
}
