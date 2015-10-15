using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Betty
{
    public static class HltvDateParser
    {
        #region Parse
        public static DateTime Parse(string date)
        {
            int year = Convert.ToInt32(Regex.Match(date, "[0-9]{4}").Value);;
            int month = GetIntMonthFromString(Regex.Match(date, 
                "January|February|March|April|May|June|July|August|September|October|November|December").Value);
            int day = Convert.ToInt32(Regex.Match(Regex.Match(date, "[0-9]{1,2}[^0-9 :]").Value, "[0-9]{1,2}").Value);
            int hour = Convert.ToInt32(Regex.Match(Regex.Match(date, "[0-9]{2}:").Value, "[0-9]{2}").Value);
            int minute = Convert.ToInt32(Regex.Match(Regex.Match(date, ":[0-9]{2}").Value, "[0-9]{2}").Value);
            return new DateTime(year, month, day, hour, minute, 0);
        }

        public static DateTime ParseLegacy(string date)
        {
            return DateTime.Now;
        }

        private static int GetIntMonthFromString(string month)
        {
            /*
             January|February|March|April|May|June|July|August|September|October|November|December
             */
            switch (month)
            { 
                case "January":
                    return 1;
                case "February":
                    return 2;
                case "March":
                    return 3;
                case "April":
                    return 4;
                case "May":
                    return 5;
                case "June":
                    return 6;
                case "July":
                    return 7;
                case "August":
                    return 8;
                case "September":
                    return 9;
                case "October":
                    return 10;
                case "November":
                    return 11;
                case "December":
                    return 12;
                default:
                    return 6;
            }
        }
        #endregion
    }
}
