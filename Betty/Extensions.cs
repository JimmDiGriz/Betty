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
    public static class Extensions
    {
        public static ListBoxItem ToListBoxItem(this object sender)
        {
            return (ListBoxItem)sender;
        }

        public static Label Tolabel(this object sender)
        {
            return (Label)sender;
        }

        public static TextBox ToTextBox(this object sender)
        { 
            return (TextBox)sender;
        }
    }
}
