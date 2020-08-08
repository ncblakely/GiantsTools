namespace Giants.DataContract
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    public class ServerInfo
    {
        [Required]
        [StringLength(100)]
        public string GameName { get; set; }

        [Required]
        [StringLength(100)]
        public string Version { get; set; }

        [Required]
        [StringLength(100)]
        public string SessionName { get; set; } // was "HostName"

        [Required]
        public int Port { get; set; }

        [Required]
        [StringLength(300)]
        public string MapName { get; set; }

        [Required]
        [StringLength(100)]
        public string GameType { get; set; }

        [Required]
        public int NumPlayers { get; set; }

        [Required]
        [StringLength(100)]
        public string GameState { get; set; }

        [Required]
        public int TimeLimit { get; set; }

        [Required]
        public int FragLimit { get; set; }

        [Required]
        public int TeamFragLimit { get; set; }

        [Required]
        public bool FirstBaseComplete { get; set; }

        [Required]
        public IList<PlayerInfo> PlayerInfo { get; set; }
    }
}
