using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoldemHand;

namespace TexasHoldemEmulator.Agents
{
    class StupidBot : BaseBot, IAgent
    {
        private static int instCount = 0;
        private ulong[] opps;
        private double[] oppsP;
        private ulong dead = 0UL;
        public StupidBot() : base()
        {
            instCount++;
            name = "StupidBot_" + instCount;
        }

        public override int GetDecision(BoardSituation situation, TableInfo info)
        {
            int inGamePlayerCount = GetNotFoldedPlayers(situation, info.Players.Keys.ToArray()).Count();
            if (info.HandId != handId)
            {
                street = -1;
                dead = hand;
                handId = info.HandId;
                opps = new ulong[info.Players.Count];
                for (int i = 0; i < opps.Length; i++)
                {
                    opps[i] = Hand.RandomHand(dead, 2);
                    dead |= opps[i];
                }
            }
            if (street != situation.Street)
            {
                street = situation.Street;
                oppsP = new double[inGamePlayerCount];
                for (int i = 0; i < oppsP.Length; i++)
                    oppsP[i] = Hand.WinOdds(opps[i], situation.Cards, dead, inGamePlayerCount);
                p = Hand.WinOdds(hand, situation.Cards, dead, inGamePlayerCount);
            }
            bool call = false, raise = false;
            for(int i = 0; i < oppsP.Length; i++)
            {
                if (oppsP[i] > p)
                    return -1;
                if (p - oppsP[i] < 0.3)
                    call = true;
                raise = true;
            }
            int minBet = situation.MaxBet - situation.GetPlayerCurrentBet(name);
            int minRaise = Convert.ToInt32((p * situation.GetPot()) + minBet);
            if (call == true && raise == false)
                return minBet;
            else if (call == true && raise == true)
                return minRaise;
            else
                return minRaise * 2;
        }
    }
}
