using System;
using System.ComponentModel.DataAnnotations;

namespace Giants.DataContract.V1
{
    public class PlayerInfo
    {
        [Required]
        public int Index { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Frags { get; set; }

        [Required]
        public int Deaths { get; set; }

        [Required]
        public string TeamName { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PlayerInfo info &&
                   Index == info.Index &&
                   Name == info.Name &&
                   Frags == info.Frags &&
                   Deaths == info.Deaths &&
                   TeamName == info.TeamName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, Name, Frags, Deaths, TeamName);
        }
    }
}
