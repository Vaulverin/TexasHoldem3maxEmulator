using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoldemHand;

namespace TexasHoldemEmulator
{
    public class BoardSituation
    {
        public readonly BoardSituation PreviousSituation;
        public readonly ulong Cards;
        public readonly int Street;
        public readonly int MaxBet = 0;
        public readonly string PlayerName;
        public readonly int Decision;
        public readonly int Deep = 0;
        public readonly int RaiseCount = 0;

        public BoardSituation(BoardSituation prevSituation, ulong cards, string playerName, int decision)
        {
            PreviousSituation = prevSituation;
            Cards = cards;
            Street = Hand.Cards(Cards).Count();
            PlayerName = playerName;
            Decision = decision;
            if (prevSituation != null)
            {
                Deep = prevSituation.Deep + 1;
                if (Cards == prevSituation.Cards)
                {
                    MaxBet = prevSituation.MaxBet;
                    RaiseCount = prevSituation.RaiseCount;
                }
                int bet = GetPlayerCurrentBet(playerName);
                if (bet > prevSituation.MaxBet)
                {
                    MaxBet = bet;
                    if (Deep > 2)
                        RaiseCount++;
                }
            }
        }

        public string GetActionString(TableInfo tInfo)
        {
            string action = "";
            int currentBet = PreviousSituation.GetPlayerCurrentBet(PlayerName);
            if (Decision == -1)
                action = "folds";
            else if (Decision == 0)
                action = "checks";
            else if (Decision <= tInfo.SmallBlind && PreviousSituation.Deep == 0)
                action = "posts small blind " + Decision;
            else if (Decision <= tInfo.SmallBlind * 2 && PreviousSituation.Deep == 1)
                action = "posts big blind " + Decision;
            else if (Decision + currentBet <= PreviousSituation.MaxBet)
                action = "calls " + Decision;
            else if (PreviousSituation.MaxBet == 0)
                action = "bets " + Decision;
            else
                action = "raises " + (Decision - (PreviousSituation.MaxBet - currentBet)) + " to " + (Decision + currentBet);
            if (tInfo.Players[PlayerName] - GetPlayerPot(PlayerName) == 0)
                action += " and is all-in";
            return action;
        }
        
        public int GetPlayerCurrentBet(string name)
        {
            int currentBet = 0;
            var node = this;
            while (node != null && Cards == node.Cards)
            {
                if (node.PlayerName == name && node.Decision > 0)
                    currentBet += node.Decision;
                node = node.PreviousSituation;
            }
            return currentBet;
        }

        public int GetActionsCount(string name)
        {
            int count = 0;
            var node = this;
            while (node != null && Cards == node.Cards)
            {
                if (node.PlayerName == name && node.Deep > 2)
                    count++;
                node = node.PreviousSituation;
            }
            return count;
        }

        public int GetPot()
        {
            int pot = 0;
            var node = this;
            while (node != null)
            {
                if (node.Decision > 0)
                    pot += node.Decision;
                node = node.PreviousSituation;
            }
            return pot;
        }

        public int GetPlayerPot(string name)
        {
            int pot = 0;
            var node = this;
            while (node != null)
            {
                if (node.PlayerName == name && node.Decision > 0)
                    pot += node.Decision;
                node = node.PreviousSituation;
            }
            return pot;
        }

        public bool IsPlayerFolded(string name)
        {
            var node = this;
            do
            {
                if (node.PlayerName == name && node.Decision == -1)
                    return true;
            } while (node = node.PreviousSituation);
            return false;
        }

        public BoardSituation GetSituationByDeep(int deep)
        {
            if (Deep == deep)
                return this;
            return PreviousSituation.GetSituationByDeep(deep);
        }

        public static implicit operator bool (BoardSituation v)
        {
            return v != null;
        }
    }

}
