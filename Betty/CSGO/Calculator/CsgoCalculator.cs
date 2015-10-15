using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class CsgoCalculator
    {
        private class TeamStat
        {
            internal class MapStat
            {
                public string Map { get; set; }
                public int Points { get; set; }
                private TeamStatParams Params { get; set; }
                public bool IsDeleted { get; set; }

                public MapStat(string map, TeamStatParams p)
                {
                    Map = map;
                    Points = 0;
                    Params = p;
                    IsDeleted = false;
                }

                public void AddPoints(int points, DateTime date)
                { 
                    if (date <= Params.BorderTime)
                    {
                        Points += Convert.ToInt32(points * Params.FarMonthCoef);
                    }
                    else if (date <= Params.Now.AddMonths(-2))
                    {
                        Points += Convert.ToInt32(points * Params.ThirdMonthCoef);
                    }
                    else if (date <= Params.Now.AddMonths(-1))
                    {
                        Points += Convert.ToInt32(points * Params.SecondMonthCoef);
                    }
                    else
                    {
                        Points += Convert.ToInt32(points * Params.FirstMonthCoef);
                    }
                }
            }

            public List<MapStat> MapStats { get; set; }
            public int Points { get; set; }
            private TeamStatParams Params { get; set; }
            public string Name { get; set; }

            public TeamStat(TeamStatParams p, string name)
            {
                MapStats = new List<MapStat>();
                Points = 0;
                Params = p;
                Name = name;
            }

            public void AddPoints(int points, DateTime date)
            {
                if (date <= Params.BorderTime)
                {
                    Points += Convert.ToInt32(points * Params.FarMonthCoef);
                }
                else if (date <= Params.Now.AddMonths(-2))
                {
                    Points += Convert.ToInt32(points * Params.ThirdMonthCoef);
                }
                else if (date <= Params.Now.AddMonths(-1))
                {
                    Points += Convert.ToInt32(points * Params.SecondMonthCoef);
                }
                else
                {
                    Points += Convert.ToInt32(points * Params.FirstMonthCoef);
                }
            }
        }

        private class TeamStatParams
        {
            public DateTime Now { get; set; }
            public DateTime BorderTime { get; set; }
            public float FirstMonthCoef { get; set; }
            public float SecondMonthCoef { get; set; }
            public float ThirdMonthCoef { get; set; }
            public float FarMonthCoef { get; set; }

            public TeamStatParams(DateTime now, DateTime border, float first,
                float second, float third, float far)
            {
                Now = now;
                BorderTime = border;
                FirstMonthCoef = first;
                SecondMonthCoef = second;
                ThirdMonthCoef = third;
                FarMonthCoef = far;
            }
        }

        #region Properties
        private List<CsgoTeam> Teams { get; set; }
        private List<CsgoMatch> Matches { get; set; }
        private string FirstTeamName { get; set; }
        private string SecondTeamName { get; set; }
        private int Maps { get; set; }
        private TeamStatParams Params { get; set; }
        private Dictionary<int, TeamStat> TeamsStats { get; set; }
        private List<string> MapPull { get; set; }
        private enum DeleteType 
        {
            SELF_WORST,
            ENEMYS_BEST,
            MIX_STYLE
        }
        #endregion

        #region Init
        public CsgoCalculator(CsgoInfo info, string firstTeam, string secondTeam, int maps)
        {
            Teams = new List<CsgoTeam>(info.Teams);
            Matches = new List<CsgoMatch>(info.Matches);
            FirstTeamName = firstTeam;
            SecondTeamName = secondTeam;
            Maps = maps;
            Params = new TeamStatParams(DateTime.Now, DateTime.Now.AddMonths(-3),
                1.5f, 0.7f, 0.5f, 0.2f);
            TeamsStats = new Dictionary<int, TeamStat>();
            MapPull = new List<string>();
        }
        #endregion

        #region Calculate
        public CsgoCalculatorResult Start()
        {
            CalculateMaps();
            DeleteMaps(DeleteType.MIX_STYLE);
            GetGamePoints();
            return ConstructResult();
        }

        private void CalculateMaps()
        {
            foreach (CsgoTeam team in Teams.
                Where(t => t.Name == FirstTeamName || t.Name == SecondTeamName))
            {
                TeamsStats.Add(team.Id, new TeamStat(Params, team.Name));

                foreach (CsgoMatch match in Matches.
                    Where(m => m.FirstTeamId == team.Id || m.SecondTeamId == team.Id))
                {
                    if (!TeamsStats[team.Id].MapStats.Exists(m => m.Map == match.Map))
                    {
                        TeamsStats[team.Id].MapStats
                            .Add(new TeamStat.MapStat(match.Map, Params));
                    }

                    //TODO: Тут очки карт скалировать от ладдера и команды.
                    int score = match.FirstTeamId == team.Id
                            ? match.FirstScore
                            : match.SecondScore;

                    //Скейлинг от команды. Если команда противника - дополнительный модификатор.
                    score = CurrentCommandScale(match, team, score);

                    //Скейлинг от ладдера на тот момент, когда происходил матч.
                    score = Convert.ToInt32((float)score * LadderScale(match, team.Id));

                    TeamsStats[team.Id].MapStats.Find(m => m.Map == match.Map)
                            .AddPoints(score, match.Date);
                }
            }
        }

        private float LadderScale(CsgoMatch match, int id)
        {
            float firstPoints = Convert.ToSingle(match.FirstPoints);
            float secondPoints = Convert.ToSingle(match.SecondPoints);

            return match.FirstTeamId == id
                ? firstPoints / secondPoints
                : secondPoints / firstPoints;
        }

        private int CurrentCommandScale(CsgoMatch match, CsgoTeam team, int score)
        {
            if (IsEnemyTeam(match, team.Name))
            {
                if (match.FirstTeamId == team.Id)
                {
                    if (match.FirstScore > match.SecondScore)
                    {
                        score = Convert.ToInt32((float)score * 1.5f);
                    }
                    else if (match.FirstScore <= match.SecondScore)
                    {
                        score = Convert.ToInt32((float)score * 0.7f);
                    }
                }
                else
                {
                    if (match.SecondScore > match.FirstScore)
                    {
                        score = Convert.ToInt32((float)score * 1.5f);
                    }
                    else if (match.SecondScore <= match.FirstScore)
                    {
                        score = Convert.ToInt32((float)score * 0.7f);
                    }
                }
            }

            return score;
        }

        private bool IsEnemyTeam(CsgoMatch match, string team)
        {
            string firstTeam = Teams.First(t => t.Name == FirstTeamName).Name;
            string secondTeam = Teams.First(t => t.Name == SecondTeamName).Name;

            string matchFirstTeam = Teams
                .First(t => t.Name == Teams.First(t2 => t2.Id == match.FirstTeamId).Name).Name;

            string matchSecondTeam = Teams
                .First(t => t.Name == Teams.First(t2 => t2.Id == match.SecondTeamId).Name).Name;

            if (matchFirstTeam == team)
            {
                return matchFirstTeam == firstTeam
                    ? matchSecondTeam == secondTeam ? true : false
                    : matchSecondTeam == firstTeam ? true : false;
            }
            else
            {
                return matchSecondTeam == firstTeam
                    ? matchSecondTeam == secondTeam ? true : false
                    : matchFirstTeam == firstTeam ? true : false;
            }
        }

        private void DeleteMaps(DeleteType type)
        {
            CreateFullPull();

            int count = MapPull.Count - Maps;
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    switch (type)
                    { 
                        case DeleteType.ENEMYS_BEST:
                            DeleteEnemysBest(GetSecondTeam().MapStats);
                            break;
                        case DeleteType.SELF_WORST:
                            DeleteSelfWorst(GetFirstTeam().MapStats);
                            break;
                        case DeleteType.MIX_STYLE:
                            DeleteMixStyle(GetFirstTeam().MapStats, true);
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case DeleteType.ENEMYS_BEST:
                            DeleteEnemysBest(GetFirstTeam().MapStats);
                            break;
                        case DeleteType.SELF_WORST:
                            DeleteSelfWorst(GetSecondTeam().MapStats);
                            break;
                        case DeleteType.MIX_STYLE:
                            DeleteMixStyle(GetSecondTeam().MapStats, false);
                            break;
                    }
                }
            }

            //FixEmptyMaps();
        }

        private void FixEmptyMaps()
        {
            foreach (string map in MapPull)
            {
                if (!TeamsStats.First().Value.MapStats.Exists(m => m.Map == map))
                {
                    TeamStat.MapStat temp = new TeamStat.MapStat(map, Params);
                    temp.Points = 1;
                    TeamsStats.First().Value.MapStats.Add(temp);
                }

                if (!TeamsStats.Last().Value.MapStats.Exists(m => m.Map == map))
                {
                    TeamStat.MapStat temp = new TeamStat.MapStat(map, Params);
                    temp.Points = 1;
                    TeamsStats.Last().Value.MapStats.Add(temp);
                }
            }
        }

        private void DeleteEnemysBest(List<TeamStat.MapStat> maps)
        {
            TeamStat.MapStat best = maps.First(m => MapPull.Contains(m.Map));

            foreach (TeamStat.MapStat map in maps.Where(m => MapPull.Contains(m.Map)))
            {
                if (!Object.ReferenceEquals(map, best))
                {
                    if (map.Points > best.Points)
                    {
                        best = map;
                    }
                }
            }

            MapPull.Remove(best.Map);
        }

        private void DeleteSelfWorst(List<TeamStat.MapStat> maps)
        {
            TeamStat.MapStat worst = maps.First(m => MapPull.Contains(m.Map));

            foreach (TeamStat.MapStat map in maps.Where(m => MapPull.Contains(m.Map)))
            {
                if (!Object.ReferenceEquals(map, worst))
                {
                    if (map.Points < worst.Points)
                    {
                        worst = map;
                    }
                }
            }

            MapPull.Remove(worst.Map);
        }

        private void DeleteMixStyle(List<TeamStat.MapStat> maps, bool isFirstTeam)
        {
            TeamStat.MapStat worst = maps.First(m => MapPull.Contains(m.Map));

            foreach (TeamStat.MapStat map in maps.Where(m => MapPull.Contains(m.Map)))
            {
                if (!Object.ReferenceEquals(map, worst))
                {
                    if (GetMapDifferece(isFirstTeam, map.Map) <
                        GetMapDifferece(isFirstTeam, worst.Map))
                    {
                        worst = map;
                    }
                }
            }

            MapPull.Remove(worst.Map);
        }

        private int GetMapDifferece(bool isFirstTeam, string map)
        {
            int firstTeamMapPoints = GetFirstTeam().MapStats
                    .First(m => m.Map == map).Points;

            int secondTeamMapPoints = GetSecondTeam().MapStats
                .First(m => m.Map == map).Points;

            if (isFirstTeam)
            {
                return firstTeamMapPoints - secondTeamMapPoints;
            }
            else
            {
                return secondTeamMapPoints - firstTeamMapPoints;
            }
        }

        private void CreateFullPull()
        {
            foreach (KeyValuePair<int, TeamStat> stat in TeamsStats)
            {
                foreach (TeamStat.MapStat map in stat.Value.MapStats)
                {
                    if (!MapPull.Contains(map.Map)) { MapPull.Add(map.Map); }
                }
            }

            if (MapPull.Contains("default")) { MapPull.Remove("default"); }
            if (MapPull.Contains("tba")) { MapPull.Remove("tba"); }
            if (MapPull.Contains("Unknown")) { MapPull.Remove("Unknown"); }

            FixEmptyMaps();
        }

        private List<TeamStat> GetTeamList()
        {
            List<TeamStat> temp = new List<TeamStat>();

            foreach (KeyValuePair<int, TeamStat> kvp in TeamsStats)
            {
                temp.Add(kvp.Value);
            }

            return temp;
        }

        private TeamStat GetFirstTeam()
        {
            return GetTeamList().Find(t => t.Name == FirstTeamName);
        }

        private TeamStat GetSecondTeam()
        {
            return GetTeamList().Find(t => t.Name == SecondTeamName);
        }

        private void GetGamePoints()
        {
            List<int> temp = new List<int>();

            foreach (KeyValuePair<int, TeamStat> kvp in TeamsStats)
            {
                temp.Add(kvp.Key);
            }

            int firstId = temp[0];
            int secondId = temp[1];

            foreach (KeyValuePair<int, TeamStat> stat in TeamsStats)
            {
                foreach (CsgoMatch match in Matches
                    .Where(m => m.FirstTeamId == stat.Key
                        || m.SecondTeamId == stat.Key && m.Date >= Params.Now.AddMonths(-3)))
                {
                    //TODO: Тут очки команды скалировать от ладдера и команды.
                    int mainMod = 2;
                    int points = stat.Key == match.FirstTeamId
                        ? match.FirstScore 
                        : match.SecondScore;
                    int enemysId = stat.Key == match.FirstTeamId 
                        ? match.SecondTeamId 
                        : match.FirstTeamId;

                    if (GetEnemysTeamId(stat.Key, match) == enemysId)
                    {
                        points *= mainMod;
                    }

                    points = Convert.ToInt32((float)points * LadderScale(match, stat.Key));

                    stat.Value.AddPoints(points, match.Date);
                }
            }
        }

        private int GetEnemysTeamId(int id, CsgoMatch match)
        {
            return match.FirstTeamId == id ? match.SecondTeamId : match.FirstTeamId;
        }

        //TODO: Вычислять значения очков команд и матчей с помощью ладдера.

        private CsgoCalculatorResult ConstructResult()
        {
            int firstTeamId = Teams.First(t => t.Name == FirstTeamName).Id;
            int secondTeamId = Teams.First(t => t.Name == SecondTeamName).Id;

            int firstTeamLadderPoints = Teams.Find(t => t.Id == firstTeamId).Points;
            int secondTeamLadderPoints = Teams.Find(t => t.Id == secondTeamId).Points;

            int firstTeamPoints = GetFirstTeam().MapStats.Sum(m => m.Points)
                + GetFirstTeam().Points;

            int secondTeamPoints = GetSecondTeam().MapStats.Sum(m => m.Points)
                + GetSecondTeam().Points;

            /*firstTeamPoints += Convert.ToInt32(((float)firstTeamPoints / 4f)
                * ((float)firstTeamLadderPoints / (float)secondTeamLadderPoints));

            secondTeamPoints += Convert.ToInt32(((float)secondTeamPoints / 4f)
                * ((float)secondTeamPoints / (float)firstTeamPoints));*/

            int firstTeamMapPointsSum = GetFirstTeam().MapStats
                .Sum(m => MapPull.Contains(m.Map) ? m.Points : 0);

            int secondTeamMapPointsSum = GetSecondTeam().MapStats
                .Sum(m => MapPull.Contains(m.Map) ? m.Points : 0);

            firstTeamPoints += firstTeamMapPointsSum;
            secondTeamPoints += secondTeamMapPointsSum;

            float firstTeamChance = 
                (float)Math.Round((float)firstTeamPoints 
                / ((float)firstTeamPoints + (float)secondTeamPoints), 2) * 100f;

            CsgoCalculatorResultBuilder builder = new CsgoCalculatorResultBuilder();

            builder.FirstName(FirstTeamName)
                .SecondName(SecondTeamName)
                .FirstId(firstTeamId)
                .SecondId(secondTeamId)
                .FirstChance(firstTeamChance);

            foreach (string map in MapPull)
            {
                float chance = (float)Math.Round((float)GetFirstTeam().MapStats
                    .Find(m => m.Map == map).Points /
                    ((float)GetSecondTeam().MapStats
                    .Find(m => m.Map == map).Points
                    + (float)GetFirstTeam().MapStats
                    .Find(m => m.Map == map).Points), 2) * 100f;

                builder.AddMap(map, chance);
            }

            //TODO: Пофиксить на расчет очков с правильных карт, а не со всех. DONE.
            //TODO: Возможно уменьшить влияние ладдерного коэффициента с трети до четверти. DONE.

            //TODO: Учитывать ладдерную позицию во времени во время расчета очков карт и командд.

            return builder.Build();
        }
        #endregion
    }
}
