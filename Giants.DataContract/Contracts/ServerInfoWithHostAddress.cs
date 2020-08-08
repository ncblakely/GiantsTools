namespace Giants.DataContract
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class ServerInfoWithHostAddress : ServerInfo
    {
        [Required]
        public string HostIpAddress { get; set; }
    }
}
