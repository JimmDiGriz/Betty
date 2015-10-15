using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class MatchInfo
    {
        #region Fields
        #endregion

        #region Properties
        public Dictionary<string, Score> Maps { get; set; }
        public string FirstTeam { get; set; }
        public string SecondTeam { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
        #endregion

        #region Init
        public MatchInfo()
        {
            Maps = new Dictionary<string, Score>();
            FirstTeam = SecondTeam = "";
        }
        #endregion

        #region Else
        public string Print()
        {
            string result = FirstTeam + " vs " + SecondTeam + "\n";

            result += "Date: " + Date + "\n";

            foreach (KeyValuePair<string, Score> kvp in Maps)
            {
                result += "Map: " + kvp.Key + " - " + kvp.Value.First + " : " + kvp.Value.Second + "\n";
            }

            return result;
        }
        #endregion
    }
}
