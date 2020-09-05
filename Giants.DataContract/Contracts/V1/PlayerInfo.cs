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
                   this.Index == info.Index &&
                   this.Name == info.Name &&
                   this.Frags == info.Frags &&
                   this.Deaths == info.Deaths &&
                   this.TeamName == info.TeamName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Index, this.Name, this.Frags, this.Deaths, this.TeamName);
        }
    }
}
