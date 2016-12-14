using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoldemHand;

namespace TexasHoldemEmulator.Agents
{
    class TightBot : BaseBot, IAgent
    {
        private static int instCount = 0;
        public TightBot()
        {
            instCount++;
            name = "TightBot_" + instCount;
        }

        public override int GetDecision(BoardSituation situation, TableInfo info)
        {
            if (handId != info.HandId)
            {
                handId = info.HandId;
                street = -1;
            }
            int pot = situation.GetPot();
            if (street != situation.Street)
            {
                p = Hand.WinOdds(hand, situation.Cards, 0UL, info.Players.Count);
                street = situation.Street;
            }
            double win = p * pot;
            int minBet = situation.MaxBet - situation.GetPlayerCurrentBet(name);
            int bet_cur = situation.GetPlayerPot(name) + minBet;
            if (bet_cur + info.SmallBlind > win && win >= bet_cur)
                return minBet;
            else if (win >= bet_cur + info.SmallBlind)
                return Convert.ToInt32((win - bet_cur) + minBet);
            return -1;
        }

    }
}
