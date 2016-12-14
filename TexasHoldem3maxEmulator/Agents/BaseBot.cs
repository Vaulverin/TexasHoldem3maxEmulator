using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemEmulator.Agents
{
    public class BaseBot : IAgent
    {
        protected string name;
        protected ulong hand = 0UL;
        protected long handId = -1;
        protected double p = 0;
        protected int street = -1;
        protected int stack = 0;
        public BaseBot()
        {
            
        }

        public virtual int GetDecision(BoardSituation situation, TableInfo info)
        {
            return -1;
        }
        
        public string GetName()
        {
            return name;
        }

        public void SetHand(ulong hand)
        {
            this.hand = hand;
        }

        protected IEnumerable<string> GetNotFoldedPlayers(BoardSituation situation, string[] players)
        {
            for (int i = 0; i < players.Length; i++)
                if (!situation.IsPlayerFolded(players[i]))
                    yield return players[i];
        }

        public void SetStack(int stack)
        {
            this.stack = stack;
        }
    }
}
