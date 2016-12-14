using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemEmulator.Agents
{
    class AgressiveBot : BaseBot, IAgent
    {
        private static int instCount = 0;
        public AgressiveBot() : base()
        {
            instCount++;
            name = "Agressive_Bot_" + instCount;
        }
        public override int GetDecision(BoardSituation situation, TableInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
