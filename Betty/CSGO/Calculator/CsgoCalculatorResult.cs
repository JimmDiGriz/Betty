using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class CsgoCalculatorResult
    {
        #region Properties
        public string FirstTeamName { get; private set; }
        public string SecondTeamName { get; private set; }
        public int FirstTeamId { get; private set; }
        public int SecondTeamId { get; private set; }
        public Dictionary<string, float> Maps { get; private set; }
        public float FirstTeamChance { get; private set; }
        #endregion

        #region Init
        public CsgoCalculatorResult(CsgoCalculatorResultBuilder builder)
        {
            FirstTeamName = builder.FirstTeamName;
            SecondTeamName = builder.SecondTeamName;
            FirstTeamId = builder.FirstTeamId;
            SecondTeamId = builder.SecondTeamId;
            Maps = builder.Maps;
            FirstTeamChance = builder.FirstTeamChance;
        }
        #endregion

        #region Else
        public string PrintResult()
        {
            string result = FirstTeamName + " VS " + SecondTeamName + "\n";
            result += FirstTeamChance + " VS " + (100f - FirstTeamChance).ToString() + "\n";
            foreach (KeyValuePair<string, float> kvp in Maps)
            {
                result += kvp.Key + " " + kvp.Value + " VS " + (100f - kvp.Value).ToString() + "\n";
            }
            return result;
        }
        #endregion
    }
}
