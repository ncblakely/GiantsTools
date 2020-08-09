namespace Giants.DataContract
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ServerInfoWithHostAddress : ServerInfo
    {
        [Required]
        public string HostIpAddress { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ServerInfoWithHostAddress address &&
                   base.Equals(obj) &&
                   HostIpAddress == address.HostIpAddress;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(HostIpAddress);
            return hash.ToHashCode();
        }
    }
}
