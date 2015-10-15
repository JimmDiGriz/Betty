using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Betty
{
    public class HltvParserResultSaver
    {
        #region Properties
        private HltvParserResult Result { get; set; }
        private BettyContextCsgo Context { get; set; }
        private Action Callback { get; set; }
        #endregion
        
        #region events
        public event ParserOutputHandler ParserOutput;
        #endregion

        #region Init
        public HltvParserResultSaver(HltvParserResult result, Action callback)
        {
            Result = result;
            Context = new BettyContextCsgo();
            Callback = callback;
        }
        #endregion

        #region Work
        public void Save()
        {
            SaveDetail();
            SaveTeams();
            SavePlayers();

            Context.SaveChanges();

            SaveMatches();

            Context.SaveChanges();

            FixErrors();
            
            Context.SaveChanges();

            if (Callback != null)
            {
                Callback();
            }
        }

        private void SaveDetail()
        {
            if (Context.csgo_parser_details.Count() == 0)
            {
                Context.csgo_parser_details.Add(new CsgoParserDetails { Url = Result.DetailsUrls.First() });
            }
            else
            {
                //Тут инфа сотка исключение будет.
                //Context.csgo_parser_details.First().Url = Result.DetailsUrls.First();
                foreach (CsgoParserDetails detail in Context.csgo_parser_details)
                {
                    if (Result.DetailsUrls.Count > 0)
                    {
                        detail.Url = Result.DetailsUrls.First();
                    }
                }
            }
            PrintOutput("End Adding Details!", 50);
        }

        private void SaveTeams()
        {
            foreach (TeamInfo team in Result.Teams)
            {
                int count = Context.csgo_team.AsEnumerable().Where(t => t.Url == team.Url).Count();
                if (count > 0)
                {
                    foreach (CsgoTeam change in Context.csgo_team.Where(t => t.Url.Equals(team.Url)))
                    {
                        change.LogoUrl = team.LogoUrl;
                        change.Name = team.Name;
                        change.Url = team.Url;
                    }
                    PrintOutput("Team " + team.Name + " Changed", 1);
                    continue;
                }

                Context.csgo_team.Add(new CsgoTeam
                {
                    Name = team.Name,
                    Url = team.Url,
                    LogoUrl = team.LogoUrl,
                    Points = 1000
                });
                PrintOutput("Added Team " + team.Name, 1);
            }
            PrintOutput("End Adding Teams!", 50);
        }

        private void SavePlayers()
        {
            foreach (PlayerInfo player in Result.Players)
            {
                int count = Context.csgo_player.AsEnumerable().Where(t => t.Url.Equals(player.Url)).Count();
                if (count > 0)
                {
                    foreach (CsgoPlayer change in Context.csgo_player.Where(t => t.Url.Equals(player.Url)))
                    {
                        change.Deaths = player.Deaths;
                        change.Kills = player.Kills;
                        change.Maps = player.Maps;
                        change.Name = player.Name;
                        change.Rounds = player.Rounds;
                        change.Team = player.Team;
                        change.Url = player.Url;
                    }
                    PrintOutput("Player " + player.Name + " Changed", 1);
                    continue;
                }

                Context.csgo_player.Add(new CsgoPlayer
                {
                    Name = player.Name,
                    Kills = player.Kills,
                    Deaths = player.Deaths,
                    Maps = player.Maps,
                    Rounds = player.Rounds,
                    Team = player.Team,
                    Url = player.Url,
                    Points = 1000
                });
                PrintOutput("Added Player " + player.Name, 1);
            }
            PrintOutput("End Adding Players!", 50);
        }

        private void SaveMatches()
        {
            foreach (MatchInfo match in Result.Matches)
            {
                foreach (KeyValuePair<string, Score> map in match.Maps)
                {
                    int firstTeamId;
                    int secondTeamId;

                    try
                    {
                        firstTeamId = Context.csgo_team.AsEnumerable()
                            .First(t => t.Name == match.FirstTeam).Id;
                    }
                    catch { firstTeamId = -1; }

                    try
                    {
                        secondTeamId = Context.csgo_team.AsEnumerable()
                            .First(t => t.Name == match.SecondTeam).Id;
                    }
                    catch { secondTeamId = -1; }

                    Context.csgo_match.Add(new CsgoMatch
                    {
                        FirstTeamId = firstTeamId,
                        SecondTeamId = secondTeamId,
                        FirstScore = map.Value.First,
                        SecondScore = map.Value.Second,
                        Map = map.Key,
                        Url = match.Url,
                        Date = match.Date
                    });
                    PrintOutput("Added Match In " + match.Date.ToString(), 1);
                }
            }
        }

        private void FixErrors()
        {
            //Поиск ошибок и исправление.
            IEnumerable<CsgoMatch> matchesWithErrors = Context.csgo_match.Where
                (m => m.FirstTeamId == -1 || m.SecondTeamId == -1);
            foreach (CsgoMatch match in matchesWithErrors)
            {
                MatchInfo temp = HltvParser.ParseMatch(match.Url);

                int firstTeamId;
                int secondTeamId;

                try
                {
                    firstTeamId = Context.csgo_team.AsEnumerable()
                        .First(t => t.Name == temp.FirstTeam).Id;
                }
                catch { firstTeamId = -1; }

                try
                {
                    secondTeamId = Context.csgo_team.AsEnumerable()
                        .First(t => t.Name == temp.SecondTeam).Id;
                }
                catch { secondTeamId = -1; }

                match.FirstTeamId = firstTeamId;
                match.SecondTeamId = secondTeamId;

                PrintOutput("Fixing Error in match " + match.Id.ToString() + "!", 50);
            }
        }

        private void PrintOutput(string message, int time)
        {
            if (ParserOutput != null)
            {
                ParserOutput(message);
                CsgoLogger.Log(message);
                Thread.Sleep(time);
            }
        }
        #endregion
    }
}
