using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public static class CsgoLadderStatic
    {
        private class TempTeam
        {
            public int Id { get; set; }
            public int Points { get; set; }
            public float Mod { get; set; }
        }

        public static int GetPointsInDate(List<CsgoMatch> matches,
            List<CsgoTeam> teams,
            CsgoTeam team, DateTime date)
        {
            List<TempTeam> tempTeams = InitiateTeams(teams);

            foreach (CsgoMatch match in matches.OrderBy(m => m.Date))
            {
                if (match.Date > date) { break; }
                if (match.FirstTeamId == -1 && match.SecondTeamId == -1) { continue; }

                TempTeam firstTeam = tempTeams.Find(t => t.Id == match.FirstTeamId);
                TempTeam secondTeam = tempTeams.Find(t => t.Id == match.SecondTeamId);

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
            }

            return tempTeams.Find(t => t.Id == team.Id).Points;
        }

        private static List<TempTeam> InitiateTeams(List<CsgoTeam> teams)
        {
            List<TempTeam> tempList = new List<TempTeam>();
            foreach (CsgoTeam team in teams)
            {
                TempTeam temp = new TempTeam();
                temp.Points = 1000;
                temp.Id = team.Id;
                tempList.Add(temp);
            }

            return tempList;
        }

        private static void CalculateMods(TempTeam first, TempTeam second)
        {
            first.Mod = (float)second.Points / (float)first.Points;
            second.Mod = (float)first.Points / (float)second.Points;
        }
    }
}
