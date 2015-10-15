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

namespace Betty
{
    public partial class MainWindow : Window
    {
        private void CsgoBtnClick(object sender, MouseButtonEventArgs e)
        {
            CsgoGrid.Visibility = Visibility.Visible;

            MainGrid.Visibility = Visibility.Hidden;
            DotaGrid.Visibility = Visibility.Hidden;
            LolGrid.Visibility = Visibility.Hidden;
            HotsGrid.Visibility = Visibility.Hidden;
            SmiteGrid.Visibility = Visibility.Hidden;
            HeartstoneGrid.Visibility = Visibility.Hidden;
            Starcraft2Grid.Visibility = Visibility.Hidden;
            BroodWarGrid.Visibility = Visibility.Hidden;
            Warcraft3Grid.Visibility = Visibility.Hidden;

            CsgoInit();
        }

        private void CsgoParserBtnClick(object sender, MouseButtonEventArgs e)
        {
            CsgoParserGrid.Visibility = Visibility.Visible;
            CsgoTeamsGrid.Visibility = Visibility.Hidden;
            CsgoPlayersGrid.Visibility = Visibility.Hidden;
            CsgoBetsGrid.Visibility = Visibility.Hidden;
        }

        private void CsgoTeamsBtnClick(object sender, MouseButtonEventArgs e)
        {
            CsgoParserGrid.Visibility = Visibility.Hidden;
            CsgoTeamsGrid.Visibility = Visibility.Visible;
            CsgoPlayersGrid.Visibility = Visibility.Hidden;
            CsgoBetsGrid.Visibility = Visibility.Hidden;
        }

        private void CsgoPlayersBtnClick(object sender, MouseButtonEventArgs e)
        {
            CsgoParserGrid.Visibility = Visibility.Hidden;
            CsgoTeamsGrid.Visibility = Visibility.Hidden;
            CsgoPlayersGrid.Visibility = Visibility.Visible;
            CsgoBetsGrid.Visibility = Visibility.Hidden;
        }

        private void CsgoBetsBtnClick(object sender, MouseButtonEventArgs e)
        {
            CsgoParserGrid.Visibility = Visibility.Hidden;
            CsgoTeamsGrid.Visibility = Visibility.Hidden;
            CsgoPlayersGrid.Visibility = Visibility.Hidden;
            CsgoBetsGrid.Visibility = Visibility.Visible;
        }
    }
}
