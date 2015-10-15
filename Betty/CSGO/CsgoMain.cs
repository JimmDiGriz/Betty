using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;

namespace Betty
{
    public partial class MainWindow : Window
    {
        private CsgoInfo CsgoInfo { get; set; }
        private CsgoCalculatorResult LastResult { get; set; }

        private void CsgoInit()
        {
            InitCsgoInfo();
            FillCsgoParserStatistics();
            FillCsgoBetsList();
            CsgoFillTeamsList(CsgoTeamsList, new RoutedEventHandler(CsgoTeamsListItemClick));
            CsgoFillTeamsList(CsgoTeamsListLeft, new RoutedEventHandler(CsgoTeamsListLeftItemClick));
            CsgoFillTeamsList(CsgoTeamsListRight, new RoutedEventHandler(CsgoTeamsListRightItemClick));
        }

        private void InitCsgoInfo()
        {
            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                List<CsgoTeam> teams = context.csgo_team.AsEnumerable().ToList();
                List<CsgoPlayer> players = context.csgo_player.AsEnumerable().ToList();
                List<CsgoMatch> matches = context.csgo_match.AsEnumerable().ToList();
                List<CsgoBets> bets = context.csgo_bets.AsEnumerable().ToList();

                CsgoInfo = new CsgoInfo(teams, players, matches, bets);

                new CsgoLadder(CsgoInfo).CalculateTeams();

                CsgoInfo.Teams = context.csgo_team.AsEnumerable().ToList();
            }
        }

        private void FillCsgoParserStatistics()
        {
            int teams = CsgoInfo.Teams.Count();
            int players = CsgoInfo.Players.Count();
            int matches = CsgoInfo.Matches.Count();
            int errors = CsgoInfo.Matches
                .Where(m => m.FirstTeamId == -1 || m.SecondTeamId == -1).Count();

            ChangeLabelContent(CsgoParserTeamsLbl, "Teams: " + teams.ToString());
            ChangeLabelContent(CsgoParserPlayersLbl, "Players: " + players.ToString());
            ChangeLabelContent(CsgoParserMatchesLbl, "Matches: " + matches.ToString());
            ChangeLabelContent(CsgoParserErrorsLbl, "Errors: " + errors.ToString());
        }

        private void CsgoFillTeamsList(ListBox box, RoutedEventHandler handler,
            List<CsgoTeam> teams = null)
        {
            if (box.Dispatcher.Thread == Thread.CurrentThread)
            {
                box.Items.Clear();
            }
            else
            {
                box.Dispatcher.BeginInvoke(new Action(delegate
                {
                    box.Items.Clear();
                }));
            }

            List<CsgoTeam> orderedTeams = null;

            if (teams == null)
            {
                orderedTeams = CsgoInfo.Teams.OrderBy(t => t.Points).Reverse().ToList();
            }
            else
            {
                orderedTeams = teams.OrderBy(t => t.Points).Reverse().ToList();
            }

            foreach (CsgoTeam team in orderedTeams)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = team.Name;
                item.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf1, 0xf1, 0xf1));
                item.FontSize = 16f;

                item.Selected += handler;
                item.MouseEnter += new MouseEventHandler(CsgoTeamsListItemEnter);
                item.MouseLeave += new MouseEventHandler(CsgoTeamsListItemLeave);

                if (box.Dispatcher.Thread == Thread.CurrentThread)
                {
                    box.Items.Add(item);
                }
                else
                {
                    box.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            box.Items.Add(item);
                        }));
                }
            }
        }

        private void CsgoTeamsListItemClick(object sender, RoutedEventArgs e)
        {
            sender.ToListBoxItem().Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3F, 0x3F, 0x41));

            CsgoTeam team = CsgoInfo.Teams
                .FirstOrDefault(t => t.Name.Equals(sender.ToListBoxItem().Content));

            CsgoTeamsTeamName.Content = team.Name + " (Id: " + team.Id + ")";
            CsgoTeamsTeamPoints.Content = "Points: " + team.Points;

            int win = 0;
            int lose = 0;
            int draw = 0;

            foreach (CsgoMatch match in CsgoInfo.Matches
                .Where(m => m.FirstTeamId == team.Id || m.SecondTeamId == team.Id))
            {
                if (team.Id == match.FirstTeamId)
                {
                    if (match.FirstScore > match.SecondScore) { win++; }
                    else if (match.FirstScore < match.SecondScore) { lose++; }
                    else { draw++; }
                }
                else if (team.Id == match.SecondTeamId)
                {
                    if (match.FirstScore < match.SecondScore) { win++; }
                    else if (match.FirstScore > match.SecondScore) { lose++; }
                    else { draw++; }
                }
            }

            CsgoTeamsMatches.Content = "Matches: " + (win + lose + draw).ToString();
            CsgoTeamsWinMatches.Content = "Wins: " + win.ToString();
            CsgoTeamsLoseMatches.Content = "Loses: " + lose.ToString();
            CsgoTeamsDrawMatches.Content = "Drawes: " + draw.ToString();
            CsgoTeamsWinPercentage.Content = "Win Percentage: "
                + Math.Round((float)((float)win / (float)(win + lose + draw)) * 100f, 2) + "%";
        }

        private void CsgoTeamsListLeftItemClick(object sender, RoutedEventArgs e)
        {
            FirstTeamLbl.Content = sender.ToListBoxItem().Content;
        }

        private void CsgoTeamsListRightItemClick(object sender, RoutedEventArgs e)
        {
            SecondTeamLbl.Content = sender.ToListBoxItem().Content;
        }

        private void SearchLeftKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender.ToTextBox().Text == "")
                {
                    CsgoFillTeamsList(CsgoTeamsListLeft,
                        new RoutedEventHandler(CsgoTeamsListLeftItemClick));
                }

                List<CsgoTeam> temp = new List<CsgoTeam>(CsgoInfo.Teams);

                List<CsgoTeam> teams = temp
                    .Where(t => t.Name.IndexOf(
                        sender.ToTextBox().Text, 
                        StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();

                CsgoFillTeamsList(CsgoTeamsListLeft,
                    new RoutedEventHandler(CsgoTeamsListLeftItemClick),
                    teams);
            }
        }

        private void SearchRightKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender.ToTextBox().Text == "")
                {
                    CsgoFillTeamsList(CsgoTeamsListRight,
                        new RoutedEventHandler(CsgoTeamsListRightItemClick));
                }

                List<CsgoTeam> temp = new List<CsgoTeam>(CsgoInfo.Teams);

                List<CsgoTeam> teams = temp
                    .Where(t => t.Name.IndexOf(
                        sender.ToTextBox().Text,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();

                CsgoFillTeamsList(CsgoTeamsListRight,
                    new RoutedEventHandler(CsgoTeamsListRightItemClick),
                    teams);
            }
        }

        private void SearchRightLostFocus(object sender, RoutedEventArgs e)
        {
            string searching = sender.ToTextBox().Text.ToLower();

            foreach (ListBoxItem item in CsgoTeamsListRight.Items)
            {
                if (item.Content.ToString().ToLower() == searching)
                {
                    SecondTeamLbl.Content = item.Content.ToString();
                    break;
                }
            }
        }

        private void CsgoTeamsListItemEnter(object sender, MouseEventArgs e)
        {
            sender.ToListBoxItem().Background = 
                new SolidColorBrush(Color.FromArgb(0xFF, 0x3F, 0x3F, 0x41));
        }

        private void CsgoTeamsListItemLeave(object sender, MouseEventArgs e)
        {
            sender.ToListBoxItem().Background = 
                new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30));
        }

        private void FillCsgoBetsList()
        {
            if (CsgoBetsList.Dispatcher.Thread == Thread.CurrentThread)
            {
                CsgoBetsList.Items.Clear();
            }
            else
            {
                CsgoBetsList.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        CsgoBetsList.Items.Clear();
                    }));
            }

            foreach (CsgoBets bet in CsgoInfo.Bets.OrderBy(b => b.Date).Reverse())
            {
                ListBoxItem item = new ListBoxItem();
                string firstName = CsgoInfo.Teams
                    .First(t => t.Id == bet.FirstTeamId).Name;

                string secondName = CsgoInfo.Teams
                    .First(t => t.Id == bet.SecondTeamId).Name;

                item.Content = bet.Id + ".(" + bet.IsTrue + ")"
                    + firstName + " vs " + secondName + " - " + bet.Chance;

                item.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf1, 0xf1, 0xf1));
                item.FontSize = 16f;

                item.Selected += new RoutedEventHandler(CsgoBetsListItemClick);
                item.MouseEnter += new MouseEventHandler(CsgoTeamsListItemEnter);
                item.MouseLeave += new MouseEventHandler(CsgoTeamsListItemLeave);

                CsgoBetsList.Items.Add(item);
            }
        }

        private void CsgoBetsListItemClick(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(sender.ToListBoxItem().Content.ToString()
                .Split(new char[]{'.'})[0]);

            CsgoBets bet = CsgoInfo.Bets.First(b => b.Id == id);

            string firstTeamName = CsgoInfo.Teams.Find(t => t.Id == bet.FirstTeamId).Name;
            string secondTeamName = CsgoInfo.Teams.Find(t => t.Id == bet.SecondTeamId).Name;

            CsgoBetResultLbl.Content = id + "\n"
                + firstTeamName + " vs " + secondTeamName + "\n";

            CsgoBetResultLbl.Content += bet.Chance + " vs " + (100f - bet.Chance) + "\n";
            CsgoBetResultLbl.Content += bet.IsTrue.ToString();

            CsgoBetsTrueBtn.Visibility = Visibility.Visible;
            CsgoBetsFalseBtn.Visibility = Visibility.Visible;
            CsgoBetsDeleteBtn.Visibility = Visibility.Visible;

            FirstTeamLbl.Content = firstTeamName;
            SecondTeamLbl.Content = secondTeamName;

            CsgoBetsCalculateBtnClick(null, null);
        }

        private void CsgoBetsTrueBtnClick(object sender, MouseButtonEventArgs e)
        {
            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                int id = Convert.ToInt32((CsgoBetsList.SelectedItem as ListBoxItem)
                    .Content.ToString().Split(new char[] { '.' })[0]);

                context.csgo_bets.First(b => b.Id == id).IsTrue = true;

                context.SaveChanges();

                CsgoInfo.Bets = context.csgo_bets.AsEnumerable().ToList();
            }

            FillCsgoBetsList();
            CsgoBetResultLbl.Content = "";
        }

        private void CsgoBetsFalseBtnClick(object sender, MouseButtonEventArgs e)
        {
            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                int id = Convert.ToInt32((CsgoBetsList.SelectedItem as ListBoxItem)
                    .Content.ToString().Split(new char[] { '.' })[0]);

                context.csgo_bets.First(b => b.Id == id).IsTrue = false;

                context.SaveChanges();

                CsgoInfo.Bets = context.csgo_bets.AsEnumerable().ToList();
            }

            FillCsgoBetsList();
            CsgoBetResultLbl.Content = "";
        }

        private void CsgoBetsDeleteBtnClick(object sender, MouseButtonEventArgs e)
        {
            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                int id = Convert.ToInt32((CsgoBetsList.SelectedItem as ListBoxItem)
                    .Content.ToString().Split(new char[] { '.' })[0]);

                context.csgo_bets.Remove(
                    context.csgo_bets.First(b => b.Id == id));

                context.SaveChanges();

                CsgoInfo.Bets = context.csgo_bets.AsEnumerable().ToList();
            }

            FillCsgoBetsList();
            CsgoBetResultLbl.Content = "";
        }

        private void CsgoParserFixBtnClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void CsgoParserConsoleOutput(string message)
        {
            if (ParserConsoleLbl.Dispatcher.Thread == Thread.CurrentThread)
            {
                ParserConsoleLbl.Content = message;
            }
            else
            {
                ParserConsoleLbl.Dispatcher.BeginInvoke(new Action(delegate
                {
                    ParserConsoleLbl.Content = message;
                }));
            }
        }

        private void CsgoParserParseBtnClick(object sender, MouseButtonEventArgs e)
        {
            ChangeLabelVisibility(CsgoParserParseBtn, Visibility.Hidden);
            ChangeCheckBoxVisiblity(TeamsCheckBox, Visibility.Hidden);
            ChangeCheckBoxVisiblity(TeamsFullCheckBox, Visibility.Hidden);
            ChangeCheckBoxVisiblity(PlayersCheckBox, Visibility.Hidden);

            HltvParser parser = new HltvParser();

            parser.Callback = () =>
            {
                HltvParserResultSaver saver = new HltvParserResultSaver(parser.Result,
                    () =>
                    {
                        ChangeLabelVisibility(CsgoParserParseBtn, Visibility.Visible);
                        ChangeCheckBoxVisiblity(TeamsCheckBox, Visibility.Visible);
                        ChangeCheckBoxVisiblity(TeamsFullCheckBox, Visibility.Visible);
                        ChangeCheckBoxVisiblity(PlayersCheckBox, Visibility.Visible);

                        CsgoInit();
                    });
                saver.ParserOutput += new ParserOutputHandler(CsgoParserConsoleOutput);
                Thread saverThread = new Thread(saver.Save);
                saverThread.SetApartmentState(ApartmentState.STA);
                saverThread.Start();
            };

            parser.ParserOutput += new ParserOutputHandler(CsgoParserConsoleOutput);

            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                if (context.csgo_parser_details.Count() != 0)
                {
                    parser.LastUrl = context.csgo_parser_details.AsEnumerable().First().Url;
                }
                else
                {
                    parser.LastUrl = null;
                }

                if ((bool)TeamsCheckBox.IsChecked && !(bool)TeamsFullCheckBox.IsChecked)
                {
                    List<string> oldTeamUrls = new List<string>();

                    foreach (CsgoTeam team in context.csgo_team)
                    {
                        oldTeamUrls.Add(team.Url);
                    }

                    parser.OldTeamUrls = oldTeamUrls;
                }
            }

            parser.DoFullTeamsParsing = (bool)TeamsFullCheckBox.IsChecked;
            parser.DoTeamsParsing = (bool)TeamsCheckBox.IsChecked;
            parser.DoPlayersParsing = (bool)PlayersCheckBox.IsChecked;

            Thread t = new Thread(parser.Start);
            t.Start();
        }

        private void CsgoBetsCalculateBtnClick(object sender, MouseButtonEventArgs e)
        {
            string firstTeam = FirstTeamLbl.Content.ToString();
            string secondTeam = SecondTeamLbl.Content.ToString();

            if (firstTeam == secondTeam) 
            {
                CsgoResultLbl.Content = "THERE ONLY ONE TEAM!";
                return;
            }

            int maps = 1;
            if ((bool)BO3.IsChecked)
            {
                maps = 3;
            }
            else if ((bool)BO5.IsChecked)
            {
                maps = 5;
            }

            LastResult = new CsgoCalculator(CsgoInfo, firstTeam, secondTeam, maps).Start();
            CsgoResultLbl.Content = LastResult.PrintResult();
            CsgoBetsSaveBtn.Visibility = Visibility.Visible;
        }

        private void CsgoBetsSaveBtnClick(object sender, MouseButtonEventArgs e)
        {
            using (BettyContextCsgo context = new BettyContextCsgo())
            {
                context.csgo_bets.Add(new CsgoBets
                    {
                        Date = DateTime.Now,
                        FirstTeamId = LastResult.FirstTeamId,
                        SecondTeamId = LastResult.SecondTeamId,
                        Chance = LastResult.FirstTeamChance,
                        IsTrue = false,
                        WinnerTeamId = LastResult.FirstTeamChance >= 50f 
                            ? LastResult.FirstTeamId
                            : LastResult.SecondTeamId
                    });
                context.SaveChanges();

                List<CsgoBets> bets = context.csgo_bets.AsEnumerable().ToList();
                CsgoInfo.Bets = bets;
            }

            FillCsgoBetsList();
            CsgoBetsSaveBtn.Visibility = Visibility.Hidden;
        }

        private void ChangeCheckBoxVisiblity(CheckBox checkBox, Visibility visibility)
        {
            if (checkBox.Dispatcher.Thread == Thread.CurrentThread)
            {
                checkBox.Visibility = visibility;
            }
            else
            {
                checkBox.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        checkBox.Visibility = visibility;
                    }));
            }
        }

        private void ChangeLabelVisibility(Label label, Visibility visibility)
        {
            if (label.Dispatcher.Thread == Thread.CurrentThread)
            {
                label.Visibility = visibility;
            }
            else
            {
                label.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        label.Visibility = visibility;
                    }));
            }
        }

        private void ChangeLabelContent(Label label, string content)
        {
            if (label.Dispatcher.Thread == Thread.CurrentThread)
            {
                label.Content = content;
            }
            else
            {
                label.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        label.Content = content;
                    }));
            }
        }
    }
}
