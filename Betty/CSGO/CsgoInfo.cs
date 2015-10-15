using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class CsgoInfo
    {
        #region Properties
        public List<CsgoTeam> Teams { get; set; }
        public List<CsgoPlayer> Players { get; set; }
        public List<CsgoMatch> Matches { get; set; }
        public List<CsgoBets> Bets { get; set; }
        #endregion

        #region Init
        public CsgoInfo(List<CsgoTeam> teams, List<CsgoPlayer> players, List<CsgoMatch> matches,
            List<CsgoBets> bets)
        {
            Teams = teams;
            Players = players;
            Matches = matches;
            Bets = bets;
        }
        #endregion
    }
}
