using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class Score
    {
        #region Fields
        #endregion

        #region Properties
        public int First { get; set; }
        public int Second { get; set; }
        #endregion

        #region Init
        public Score(int first, int second)
        {
            First = first;
            Second = second;
        }
        public Score()
        {
            First = Second = 0;
        }
        #endregion
    }
}
