using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class CsgoLadder
    {
        private class TempTeam
        {
            public int Points { get; set; }
            public float Mod { get; set; }
        }

        #region Properties
        private List<CsgoTeam> Teams { get; set; }
        private List<CsgoMatch> Matches { get; set; }
        private List<CsgoPlayer> Players { get; set; }
        private float BigMod { get; set; }
        private float SmallMod { get; set; }
        #endregion

        #region Init
        public CsgoLadder(CsgoInfo info)
        {
            Teams = info.Teams;
            Matches = new List<CsgoMatch>(info.Matches);
            Matches.Reverse();
            Players = info.Players;
        }
        #endregion

        #region Calculate
        public void CalculateAll()
        {
            CalculateTeams();
            CalculatePlayers();
        }

        public void CalculateTeams()
        {
            ClearTeamPoints();

            foreach (CsgoMatch match in Matches)
            {
                CsgoTeam firstTeamTemp = null;
                CsgoTeam secondTeamTemp = null;

                if (match.FirstTeamId != -1)
                {
                    try
                    {
                        firstTeamTemp = Teams
                            .First(t => t.Id.Equals(match.FirstTeamId));
                    }
                    catch { firstTeamTemp = null; }
                }

                if (match.SecondTeamId != -1)
                {
                    try
                    {
                        secondTeamTemp = Teams
                            .FirstOrDefault(t => t.Id.Equals(match.SecondTeamId));
                    }
                    catch { secondTeamTemp = null; }
                }

                if (firstTeamTemp == null && secondTeamTemp == null) { continue; }

                TempTeam firstTeam = InitiateTeam(firstTeamTemp);
                TempTeam secondTeam = InitiateTeam(secondTeamTemp);

                CalculateMods(firstTeam, secondTeam);

                if (match.FirstScore == 0) { match.FirstScore = 1; }
                if (match.SecondScore == 0) { match.SecondScore = 1; }

                int points = 0;
                if (match.FirstScore >= match.SecondScore)
                {
                    points = 10 * Convert.ToInt32(
                        ((float)match.FirstScore / (float)match.SecondScore) * firstTeam.Mod);
                    firstTeam.Points += points;
                    secondTeam.Points -= points;
                }
                else
                {
                    points = 10 * Convert.ToInt32(
                        ((float)match.SecondScore / (float)match.FirstScore) * secondTeam.Mod);
                    secondTeam.Points += points;
                    firstTeam.Points -= points;
                }

                if (firstTeamTemp != null)
                {
                    firstTeamTemp.Points = firstTeam.Points;
                }

                if (secondTeamTemp != null)
                {
                    secondTeamTemp.Points = secondTeam.Points;
                }

                match.FirstPoints = firstTeam.Points;
                match.SecondPoints = secondTeam.Points;
            }

            SaveTeams();
        }

        private TempTeam InitiateTeam(CsgoTeam team)
        {
            TempTeam temp = new TempTeam();

            if (team != null)
            {
                temp.Points = team.Points;
            }
            else
            {
                temp.Points = 1000;
            }

            return temp;
        }

        private void CalculateMods(TempTeam first, TempTeam second)
        {
            first.Mod = (float)second.Points / (float)first.Points;
            second.Mod = (float)first.Points / (float)second.Points;
        }

        private void SaveTeams()
        {
            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                foreach (CsgoTeam team in Teams)
                {
                    context.csgo_team.Find(team.Id).Points = team.Points;
                }

                foreach (CsgoMatch match in context.csgo_match)
                {
                    match.FirstPoints = Matches.Find(m => m.Id == match.Id).FirstPoints;
                    match.SecondPoints = Matches.Find(m => m.Id == match.Id).SecondPoints;
                }

                context.SaveChanges();
            }
        }

        private void ClearTeamPoints()
        {
            foreach (CsgoTeam team in Teams)
            {
                team.Points = 1000;
            }
        }

        public void CalculatePlayers()
        { 
            
        }
        #endregion
    }
}
