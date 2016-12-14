using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoldemHand;

namespace TexasHoldemEmulator.Agents
{
    public interface IAgent
    {
        int GetDecision(BoardSituation situation, TableInfo info);
        string GetName();
        void SetHand(ulong hand);
        void SetStack(int stack);
    }
}
