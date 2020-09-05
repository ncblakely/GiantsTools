namespace Giants.DataContract.V1
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class ServerInfo
    {
        [Required]
        [StringLength(100)]
        public string GameName { get; set; }

        [Required]
        public AppVersion Version { get; set; }

        [Required]
        [StringLength(100)]
        public string SessionName { get; set; }

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

        public override bool Equals(object obj)
        {
            return obj is ServerInfo info &&
                   this.GameName == info.GameName &&
                   EqualityComparer<AppVersion>.Default.Equals(this.Version, info.Version) &&
                   this.SessionName == info.SessionName &&
                   this.Port == info.Port &&
                   this.MapName == info.MapName &&
                   this.GameType == info.GameType &&
                   this.NumPlayers == info.NumPlayers &&
                   this.GameState == info.GameState &&
                   this.TimeLimit == info.TimeLimit &&
                   this.FragLimit == info.FragLimit &&
                   this.TeamFragLimit == info.TeamFragLimit &&
                   this.FirstBaseComplete == info.FirstBaseComplete &&
                   this.PlayerInfo.SequenceEqual(info.PlayerInfo);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(this.GameName);
            hash.Add(this.Version);
            hash.Add(this.SessionName);
            hash.Add(this.Port);
            hash.Add(this.MapName);
            hash.Add(this.GameType);
            hash.Add(this.NumPlayers);
            hash.Add(this.GameState);
            hash.Add(this.TimeLimit);
            hash.Add(this.FragLimit);
            hash.Add(this.TeamFragLimit);
            hash.Add(this.FirstBaseComplete);
            hash.Add(this.PlayerInfo);
            return hash.ToHashCode();
        }
    }
}
