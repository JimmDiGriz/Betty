using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class HltvParserResult
    {
        #region Properties
        public List<MatchInfo> Matches { get; set; }
        public List<TeamInfo> Teams { get; set; }
        public List<PlayerInfo> Players { get; set; }
        public List<string> DetailsUrls { get; set; }
        #endregion

        #region Init
        public HltvParserResult(List<MatchInfo> matches, List<TeamInfo> teams, List<PlayerInfo> players, List<string> detailsUrls)
        {
            Matches = matches;
            Teams = teams;
            Players = players;
            DetailsUrls = detailsUrls;
        }
        #endregion
    }
}
