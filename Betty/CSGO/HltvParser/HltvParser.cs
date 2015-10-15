using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace Betty
{
    public class HltvParser
    {
        #region Properties
        private HtmlDocument Doc { get; set; }
        private int PageNumber { get; set; }
        private int PageStep { get; set; }
        private string HltvUrl { get; set; }
        private string HltvResultsUrl { get; set; }
        private string HltvTeamsUrl { get; set; }
        private List<string> DetailsUrls { get; set; }
        private Dictionary<string, string> TeamLinks { get; set; }
        private List<MatchInfo> Matches { get; set; }
        private Dictionary<string, string> PlayerLinks { get; set; }
        private List<TeamInfo> Teams { get; set; }
        private List<PlayerInfo> Players { get; set; }
        public string LastUrl { get; set; }

        public HltvParserResult Result { get; set; }
        public Action Callback { get; set; }
        public bool DoTeamsParsing { get; set; }
        public bool DoPlayersParsing { get; set; }
        public List<string> OldTeamUrls { get; set; }
        public bool DoFullTeamsParsing { get; set; }
        #endregion

        #region events
        public event ParserOutputHandler ParserOutput;
        #endregion

        #region Init
        public HltvParser(Action callback = null, string lastUrl = null)
        {
            Doc = new HtmlDocument();
            PageNumber = 0;
            PageStep = 50;
            HltvUrl = "http://www.hltv.org";
            HltvResultsUrl = "http://www.hltv.org/results";
            HltvTeamsUrl = "http://www.hltv.org/?pageid=182&mapCountOverride=0";
            DetailsUrls = new List<string>();
            TeamLinks = new Dictionary<string, string>();
            Matches = new List<MatchInfo>();
            PlayerLinks = new Dictionary<string, string>();
            Teams = new List<TeamInfo>();
            Players = new List<PlayerInfo>();
            OldTeamUrls = new List<string>();

            Callback = callback;
            LastUrl = lastUrl;
            DoTeamsParsing = DoPlayersParsing = DoFullTeamsParsing = false;
        }
        #endregion

        #region Parse
        public void Start()
        {
            StartParse();
        }

        private void StartParse()
        {
            GetDetailsUrls();
            ParseDetails();
            if (DoTeamsParsing)
            {
                if (DoFullTeamsParsing)
                {
                    ParseTeamLinks();
                }
                ParseTeams();
            }
            if (DoPlayersParsing)
            {
                ParsePlayerLinks();
            }

            Result = new HltvParserResult(Matches, Teams, Players, DetailsUrls);

            if (Callback != null)
            {
                Callback();
            }

            //ParseDetailsLegacy("http://www.hltv.org/legacy/match/2292334-global-vexx-cevo-professional-season-5");
        }

        private void GetDetailsUrls()
        {
            IEnumerable<HtmlNode> temp;
            int a = 0;
            bool stop = false;
            PrintOutput("Get Details Urls", 1);

            do
            {
                Doc.LoadHtml(GetStringFromUrl(HltvResultsUrl + "/" + PageNumber + "/"));
                temp = GetByClass("div", "matchActionCell");

                foreach (HtmlNode node in temp)
                {
                    if (LastUrl != null)
                    {
                        //TODO: Выдирать айдишники матчей тут надо
                        if (LastUrl != HltvUrl + node.FirstChild.Attributes["href"].Value)
                        {
                            DetailsUrls.Add(HltvUrl + node.FirstChild.Attributes["href"].Value);
                            PrintOutput(++a + "\t" + DetailsUrls[a - 1], 1);
                        }
                        else
                        {
                            stop = true;
                            break;
                        }
                    }
                    else
                    {
                        DetailsUrls.Add(HltvUrl + node.FirstChild.Attributes["href"].Value);
                        PrintOutput(++a + "\t" + DetailsUrls[a - 1], 1);
                    }
                }

                PageNumber += PageStep;
            } while (temp.Count() != 0 && !stop);

            PrintOutput("All Details Urls Getted!", 1);
        }

        //http://www.hltv.org/legacy/match/2292334-global-vexx-cevo-professional-season-5
        private void ParseDetailsLegacy(string url)
        {
            Doc.LoadHtml(GetStringFromUrl(url));

            MatchInfo match = new MatchInfo();
            match.Url = url;
            SetDateLegacy(match);
            SetMapAndScoreLegacy(match);
            SetTeamNamesLegacy(match);

            Matches.Add(match);

            PrintOutput(match.Print(), 1);
        }

        private void SetDateLegacy(MatchInfo match)
        {
            string date = Doc.DocumentNode.Descendants("td").Where(n =>
                n.InnerText == "Time:").First().NextSibling.InnerText;
        }

        private void SetMapAndScoreLegacy(MatchInfo match)
        { 
            
        }

        private void SetTeamNamesLegacy(MatchInfo match)
        { 
            
        }

        private void ParseDetails()
        {
            foreach (string url in DetailsUrls)
            {
                if (url == "http://www.hltv.org/match/")
                {
                    continue;
                }

                if (url == "http://www.hltv.org/match/2292334-global-vexx-cevo-professional-season-5")
                {
                    //ParseDetailsLegacy(url);
                    break;
                }
                Doc.LoadHtml(GetStringFromUrl(url));

                MatchInfo match = new MatchInfo();
                match.Url = url;
                SetDate(match);
                SetMapAndScore(match);
                SetTeamNames(match);

                Matches.Add(match);

                PrintOutput(match.Print(), 1);
            }
        }

        public static MatchInfo ParseMatch(string url)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(new WebClient().DownloadString(url));

            MatchInfo match = new MatchInfo();
            match.Url = url;

            IEnumerable<HtmlNode> tempNames = doc.DocumentNode.Descendants("a").
                Where(n => n.Attributes.Contains("class")
                    && n.Attributes["class"].Value.Contains("nolinkstyle"));

            match.FirstTeam = tempNames.First().InnerText.Replace("amp;", "");
            match.SecondTeam = tempNames.ElementAt(1).InnerText.Replace("amp;", "");

            return match;
        }

        private void SetMapAndScore(MatchInfo match)
        {
            IEnumerable<HtmlNode> temp = GetByClass("div", "hotmatchbox")
                .Where(n => n.Attributes.Contains("style")
                    && n.Attributes["style"].Value.Contains("margin-top: -7px;font-size: 12px;width:270px;border-top:0;"));

            List<Score> tempScore = new List<Score>();
            foreach (HtmlNode node in temp)
            {
                tempScore.Add(new Score(Convert.ToInt32(node.ChildNodes[1].InnerText),
                    Convert.ToInt32(node.ChildNodes[3].InnerText)));
            }

            IEnumerable<HtmlNode> maps = Doc.DocumentNode.Descendants("img").Where(n =>
                n.Attributes.Contains("src") && n.Attributes["src"].Value.Contains("images/hotmatch")
                && n.Attributes.Contains("style") && !n.Attributes["style"].Value.Contains("opacity:0.4"));

            int i = 0;
            foreach (HtmlNode node in maps)
            {
                try
                {
                    string map = Regex.Match(
                        Regex.Match(node.Attributes["src"].Value, "[a-zA-Z0-9_-]*[.]png|jpg|bmp|tiff|tga|gif")
                        .Value, "[a-zA-Z0-9-_]{4,}").Value;
                    if (map == "")
                    {
                        map = "tba";
                    }

                    match.Maps.Add(map, tempScore[i]);
                }
                catch
                {
                    try
                    {
                        match.Maps.Add("Unknown", tempScore[i]);
                    }
                    catch
                    {
                        continue;
                    }
                }
                i++;
            }
        }

        private void SetTeamNames(MatchInfo match)
        {
            IEnumerable<HtmlNode> tempNames = GetByClass("a", "nolinkstyle");

            match.FirstTeam = tempNames.First().InnerText.Replace("amp;", "");
            match.SecondTeam = tempNames.ElementAt(1).InnerText.Replace("amp;", "");

            //TODO: Выдирать линки на команды из странцы матча.
            //TODO: Исключать повторения, включая те линки, которые получены из таблицы.

            if (!TeamLinks.ContainsKey(match.FirstTeam))
            {
                if (tempNames.First().Attributes.Contains("href"))
                {
                    TeamLinks.Add(match.FirstTeam,
                        HltvUrl + "/" + tempNames.First().Attributes["href"].Value.Replace("amp;", ""));
                }
            }

            if (!TeamLinks.ContainsKey(match.SecondTeam))
            {
                if (tempNames.ElementAt(1).Attributes.Contains("href"))
                {
                    TeamLinks.Add(match.SecondTeam,
                        HltvUrl + "/" + tempNames.ElementAt(1).Attributes["href"].Value.Replace("amp;", ""));
                }
            }
        }

        private void SetDate(MatchInfo match)
        {
            try
            {
                match.Date = HltvDateParser.Parse(Doc.DocumentNode.Descendants("span").Where(n => n.Attributes.Contains("style")
                    && n.Attributes["style"].Value.Contains("font-size:14px;")).First().ParentNode.InnerText);
            }
            catch
            {
                try
                {
                    match.Date = HltvDateParser.Parse(Doc.DocumentNode.Descendants("div").Where(n =>
                        n.Attributes.Contains("style") && n.Attributes["style"].Value.Contains("text-align:center;font-size: 18px;"))
                        .First().InnerText);
                }
                catch
                {
                    match.Date = new DateTime(1990, 1, 1, 0, 0, 0);
                }
            }
        }

        private void ParseTeamLinks()
        {
            Doc.LoadHtml(GetStringFromUrl(HltvTeamsUrl));

            IEnumerable<HtmlNode> temp = Doc.DocumentNode.Descendants("a").Where(n =>
                n.Attributes.Contains("href") && n.Attributes["href"].Value.Contains("teamid"));

            foreach (HtmlNode node in temp)
            {
                if (!DoFullTeamsParsing && OldTeamUrls.Count > 0)
                {
                    if (OldTeamUrls.Count
                        (url => url == HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", "")) 
                        > 0)
                    {
                        continue;
                    }
                }

                if (TeamLinks.ContainsKey(node.InnerText)) { continue; }
                if (TeamLinks
                    .ContainsValue
                        (HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", ""))) 
                { continue; }

                try
                {
                    TeamLinks.Add(node.InnerText, HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", ""));
                    PrintOutput(node.InnerText + ": " + HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", ""), 1);
                }
                catch (ArgumentException ex)
                {
                    TeamLinks.Add(node.InnerText + CountSameTeams(node.InnerText),
                        HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", ""));
                    PrintOutput(node.InnerText + CountSameTeams(node.InnerText) + ": " + HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", ""), 1);
                }
            }

            PrintOutput("Parse Team Links Completed!", 1);
        }

        private void ParseTeams()
        {
            foreach (KeyValuePair<string, string> NameAndLink in TeamLinks)
            {
                Doc.LoadHtml(GetStringFromUrl(NameAndLink.Value));

                string logo = Doc.DocumentNode.Descendants("img").Where(n =>
                    n.Attributes.Contains("alt") && n.Attributes["alt"].Value.Contains("logo")).First().Attributes["src"].Value;

                Teams.Add(new TeamInfo(NameAndLink.Key, NameAndLink.Value, logo));

                IEnumerable<HtmlNode> temp = Doc.DocumentNode.Descendants("a").Where(n =>
                    n.Attributes.Contains("href") && n.Attributes["href"].Value.Contains("playerid"));

                foreach (HtmlNode node in temp)
                {
                    try
                    {
                        PlayerLinks.Add(node.InnerText.Remove(node.InnerText.IndexOf(" (")), HltvUrl + "/" + node.Attributes["href"].Value.Replace("amp;", ""));
                    }
                    catch
                    {
                        continue;
                    }
                }

                PrintOutput("Parsing Team " + NameAndLink.Key + " Complete " + PlayerLinks.Count, 1);
            }

            PrintOutput("Parsing Teams Completed!", 50);
        }

        private void ParsePlayerLinks()
        {
            foreach (KeyValuePair<string, string> NameAndLink in PlayerLinks)
            {
                Doc.LoadHtml(GetStringFromUrl(NameAndLink.Value));

                string name = NameAndLink.Key;

                string team = Doc.DocumentNode.Descendants("a").Where(n =>
                    n.Attributes.Contains("href") && n.Attributes["href"].Value.Contains("teamid")).First().InnerText;

                IEnumerable<HtmlNode> temp = Doc.DocumentNode.Descendants("div").Where(n =>
                    n.Attributes.Contains("style")
                    && n.Attributes["style"].Value.Contains("font-weight:normal;width:100px;float:left;text-align:right;color:black"));

                HtmlNode[] stats = temp.ToArray();

                int kills = Convert.ToInt32(stats[0].InnerText);
                float hs = Convert.ToSingle(stats[1].InnerText.Replace("%", ""), new CultureInfo("en-US"));
                int deaths = Convert.ToInt32(stats[2].InnerText);
                int maps = Convert.ToInt32(stats[4].InnerText);
                int rounds = Convert.ToInt32(stats[5].InnerText);

                PlayerInfo info = new PlayerInfo(name, kills, deaths, hs, team, rounds, maps, NameAndLink.Value);

                Players.Add(info);

                PrintOutput(info.Print(), 1);
            }
        }
        #endregion

        #region Helpers
        private string GetStringFromUrl(string url)
        {
            return new WebClient().DownloadString(url);
        }

        private IEnumerable<HtmlNode> GetByClass(string tagName, string className)
        {
            return Doc.DocumentNode.Descendants(tagName).Where(d =>
                        d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className));
        }

        private void PrintPlayerLinks()
        {
            foreach (KeyValuePair<string, string> kvp in PlayerLinks)
            {
                Console.WriteLine("Name: " + kvp.Key);
                Console.WriteLine("Link: " + kvp.Value);
            }
        }

        private void PrintPlayerInfos()
        {
            foreach (PlayerInfo player in Players)
            {
                player.Print();
            }
        }

        private int CountSameTeams(string team)
        {
            return TeamLinks.Count(m => m.Key.Contains(team));
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