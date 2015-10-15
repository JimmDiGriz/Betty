using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private void ExitBtnEnter(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://siteoforigin:,,,/Images/OnOverCross.png";
            ExitBtn.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void ExitBtnLeave(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://siteoforigin:,,,/Images/Cross.png";
            ExitBtn.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void GreyBtnEnter(object sender, MouseEventArgs e)
        {
            //3f3f41
            sender.Tolabel().Background = 
                new SolidColorBrush(Color.FromArgb(0xFF, 0x3F, 0x3F, 0x41));
        }

        private void GreyBtnLeave(object sender, MouseEventArgs e)
        {
            //2d2d30
            sender.Tolabel().Background = 
                new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30));
        }

        private void GreenBtnEnter(object sender, MouseEventArgs e)
        {
            //7ea64b
            sender.Tolabel().Background = 
                new SolidColorBrush(Color.FromArgb(0xFF, 0x7e, 0xa6, 0x4b));
        }

        private void GreenBtnLeave(object sender, MouseEventArgs e)
        {
            //2d2d30
            sender.Tolabel().Background = 
                new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30));
        }

        private void BlueBtnEnter(object sender, MouseEventArgs e)
        { 
        
        }

        private void BlueBtnLeave(object sender, MouseEventArgs e)
        { 
        
        }

        private void RedBtnEnter(object sender, MouseEventArgs e)
        { 
        
        }

        private void RedBtnLeave(object sender, MouseEventArgs e)
        { 
        
        }

        private void YellowBtnEnter(object sender, MouseEventArgs e)
        { 
        
        }

        private void YellowBtnLeave(object sender, MouseEventArgs e)
        { 
            
        }
    }
}
