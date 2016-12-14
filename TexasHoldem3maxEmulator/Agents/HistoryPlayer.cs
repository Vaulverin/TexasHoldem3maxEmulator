using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemEmulator.Agents
{
    class HistoryPlayer : BaseBot, IAgent
    {
        private static int instCount = 0;
        private int[] Decisions;
        private int decisionIndex = 0;
        public HistoryPlayer(int[] decisions, string nick = "") : base()
        {
            instCount++;
            if (nick == "")
                name = "History Player " + instCount;
            else
                name = nick;
            Decisions = decisions;
        }

        public override int GetDecision(BoardSituation situation, TableInfo info)
        {
            if (decisionIndex + 1 > Decisions.Length)
                return -1;
            return Decisions[decisionIndex++];
        }
    }
}
