using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class TeamInfo
    {
        #region Properties
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string Url { get; set; }
        #endregion

        #region Init
        public TeamInfo(string name, string url, string logo = null)
        {
            Name = name;
            LogoUrl = logo;
            Url = url;
        }
        #endregion

        #region Else
        public void Print()
        {
            Console.WriteLine("Name: " + Name);
        }
        #endregion
    }
}
