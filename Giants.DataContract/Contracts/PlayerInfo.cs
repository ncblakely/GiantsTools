using System;

namespace Giants.DataContract
{
    public class PlayerInfo
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public int Frags { get; set; }

        public int Deaths { get; set; }

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
