using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class PlayerInfo
    {
        #region Fields
        #endregion

        #region Properties
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public float Headshots { get; set; }
        public string Team { get; set; }
        public int Rounds { get; set; }
        public int Maps { get; set; }
        public string Url { get; set; }
        #endregion

        #region Init
        public PlayerInfo(string name, int kills, int deaths, float hs, string team, int rounds, int maps, string url)
        {
            Name = name;
            Kills = kills;
            Deaths = deaths;
            Headshots = hs;
            Team = team;
            Rounds = rounds;
            Maps = maps;
            Url = url;
        }
        #endregion

        #region Else
        public string Print()
        {
            string result = "Name: " + Name;
            result += "Kills: " + Kills;
            result += "Deaths: " + Deaths;
            result += "Headshots: " + Headshots;
            result += "Team: " + Team;
            result += "Rounds: " + Rounds;
            result += "Maps: " + Maps;

            return result;
        }
        #endregion
    }
}
