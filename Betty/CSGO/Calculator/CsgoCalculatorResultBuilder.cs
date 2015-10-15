using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betty
{
    public class CsgoCalculatorResultBuilder
    {
        #region Properties
        public string FirstTeamName { get; set; }
        public string SecondTeamName { get; set; }
        public int FirstTeamId { get; set; }
        public int SecondTeamId { get; set; }
        public Dictionary<string, float> Maps { get; set; }
        public float FirstTeamChance { get; set; }
        #endregion

        #region Init
        public CsgoCalculatorResultBuilder()
        {
            Maps = new Dictionary<string, float>();
        }

        public CsgoCalculatorResultBuilder FirstName(string name)
        {
            FirstTeamName = name;
            return this;
        }

        public CsgoCalculatorResultBuilder SecondName(string name)
        {
            SecondTeamName = name;
            return this;
        }

        public CsgoCalculatorResultBuilder FirstId(int id)
        {
            FirstTeamId = id;
            return this;
        }

        public CsgoCalculatorResultBuilder SecondId(int id)
        {
            SecondTeamId = id;
            return this;
        }

        public CsgoCalculatorResultBuilder AddMap(string map, float firstTeamChance)
        {
            Maps.Add(map, firstTeamChance);
            return this;
        }

        public CsgoCalculatorResultBuilder FirstChance(float chance)
        {
            FirstTeamChance = chance;
            return this;
        }

        public CsgoCalculatorResult Build()
        {
            return new CsgoCalculatorResult(this);
        }
        #endregion
    }
}
