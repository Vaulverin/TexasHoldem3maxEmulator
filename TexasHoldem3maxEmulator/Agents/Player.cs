using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemEmulator.Agents
{
    class Player : BaseBot, IAgent
    {
        private static int instCount = 0;
        public Player() : base()
        {
            instCount++;
            name = "Player" + instCount;
        }
        public override int GetDecision(BoardSituation situation, TableInfo info)
        {
            Console.WriteLine(name + " your turn, make decision");
            var strBet = Console.ReadLine();
            return Convert.ToInt32(strBet);
        }
    }
}
