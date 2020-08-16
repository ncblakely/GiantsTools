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
                   GameName == info.GameName &&
                   EqualityComparer<AppVersion>.Default.Equals(Version, info.Version) &&
                   SessionName == info.SessionName &&
                   Port == info.Port &&
                   MapName == info.MapName &&
                   GameType == info.GameType &&
                   NumPlayers == info.NumPlayers &&
                   GameState == info.GameState &&
                   TimeLimit == info.TimeLimit &&
                   FragLimit == info.FragLimit &&
                   TeamFragLimit == info.TeamFragLimit &&
                   FirstBaseComplete == info.FirstBaseComplete &&
                   PlayerInfo.SequenceEqual(info.PlayerInfo);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GameName);
            hash.Add(Version);
            hash.Add(SessionName);
            hash.Add(Port);
            hash.Add(MapName);
            hash.Add(GameType);
            hash.Add(NumPlayers);
            hash.Add(GameState);
            hash.Add(TimeLimit);
            hash.Add(FragLimit);
            hash.Add(TeamFragLimit);
            hash.Add(FirstBaseComplete);
            hash.Add(PlayerInfo);
            return hash.ToHashCode();
        }
    }
}
