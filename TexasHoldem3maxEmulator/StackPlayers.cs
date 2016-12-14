using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoldemHand;

namespace TexasHoldemEmulator
{
    public class StackPlayers
    {
        private List<EmulatorPlayer> players { get { return allPlayers.Where(p => p.InGame).ToList(); } }
        private List<EmulatorPlayer> allPlayers = new List<EmulatorPlayer>();
        private int button = 0;
        public int Button { get { return button; } }

        public int Count { get { return players.Count; } }

        public StackPlayers(List<EmulatorPlayer> players)
        {
            allPlayers = players;
        }
        public void SetRandomButton()
        {
            button = new Random().Next(0, players.Count);
        }
        public void MoveButton()
        {
            button = (button + 1) % players.Count;
        }
        public Dictionary<string, int> Players
        {
            get
            {
                return players.Select(p => new { name = p.Agent.GetName(), stack = p.Stack }).ToDictionary(e => e.name, e => e.stack);
            }
        }

        public EmulatorPlayer this[int key]
        {
            get { return players[key]; }
        }

        public EmulatorPlayer GetNextPlayer(BoardSituation currentSituation, TableInfo tInfo)
        {
            // Меняем стэк сходившего игрока
            if (currentSituation.Decision > 0)
                players[GetIndexPlayerByName(currentSituation.PlayerName)].Stack -= currentSituation.Decision;
            else if (currentSituation.Decision == -1 && currentSituation.PlayerName != "")
                players[GetIndexPlayerByName(currentSituation.PlayerName)].Folded = true;

            int sbPlayer = 0;
            if (players.Count > 2)
                sbPlayer = GetNextIndexInGame(tInfo.Button);
            else
                sbPlayer = tInfo.Button;
            if (currentSituation.Deep == 0)
                return players[sbPlayer];
            else if (currentSituation.Deep == 1)
                return players[GetNextIndexInGame(sbPlayer)];

            // Если игроков в игре и не сбросивших меньше 2х, круг закончен
            if (players.Count(p => p.Folded == false) < 2)
                return null;

            int curIndex = GetNextIndexInGame(currentSituation.PlayerName);
            int? prevIndex = null;
            if (currentSituation.PlayerName == "")
                curIndex = GetNextIndexInGame(tInfo.Button);
            else
                prevIndex = GetIndexPlayerByName(currentSituation.PlayerName);
            // Если текущий игрок выставился, берем следующего
            while (prevIndex != curIndex && (players[curIndex].Stack == 0 || players[curIndex].Folded))
            {
                if (prevIndex == null)
                    prevIndex = curIndex;
                curIndex = GetNextIndexInGame(curIndex);
            }
            if (curIndex == prevIndex)
                return null;
            int curBet = currentSituation.GetPlayerCurrentBet(players[curIndex].Agent.GetName());
            // Если ставки заколированы и игрок уже ходил, круг закончен
            if (curBet == currentSituation.MaxBet && (currentSituation.GetActionsCount(players[curIndex].Agent.GetName()) > 0
                || players.Count(p => p.Stack > 0 && p.Folded == false) <= 1))
                return null;
            return players[curIndex];
        }

        public bool IsGameOver(BoardSituation currentSituation, TableInfo tInfo, ref TableLog log)
        {
            if (players.Count(p => !p.Folded) < 2 || Hand.Cards(currentSituation.Cards).Count() == 5)
            {
                int pot = currentSituation.GetPot();
                var lastPlayers = players.Where(p => !p.Folded).ToList();
                if (lastPlayers.Count == 1)
                {
                    lastPlayers[0].Stack += pot;
                    log.Add(GameState.ENDINFO, lastPlayers[0].Agent.GetName() + " won " + pot + " chips.");
                }
                else
                {
                    var bets = new List<Bets>();
                    for (int i = 0; i < lastPlayers.Count; i++)
                    {
                        var b = new Bets();
                        b.HandValue = Hand.Evaluate(currentSituation.Cards | lastPlayers[i].Hand);
                        b.Bet = currentSituation.GetPlayerPot(lastPlayers[i].Agent.GetName());
                        b.Name = lastPlayers[i].Agent.GetName();
                        bets.Add(b);
                    }
                    var foldedPlayers = players.Where(p => p.Folded).ToList();
                    var foldedBets = new List<int>();
                    for (int i = 0; i < foldedPlayers.Count; i++)
                        foldedBets.Add(currentSituation.GetPlayerPot(foldedPlayers[i].Agent.GetName()));
                    
                    while (pot > 0)
                    {
                        var max = bets.Max(b => b.HandValue);
                        var maxValues = bets.Where(b => b.HandValue == max).OrderBy(b => b.Bet).ToList();
                        int win = maxValues[0].Bet;
                        int foldedPot = 0;
                        for (int i = 0; i < foldedBets.Count; i++)
                            if (foldedBets[i] < win)
                            {
                                foldedPot += foldedBets[i];
                                foldedBets[i] = 0;
                            }
                            else
                            {
                                foldedPot += win;
                                foldedBets[i] -= win;
                            }
                        int sidePot = 0;
                        for (int i = 0; i < bets.Count; i++)
                            if (bets[i].Name != maxValues[0].Name)
                                if (bets[i].Bet < win)
                                {
                                    sidePot += bets[i].Bet;
                                    bets[i].Bet = 0;
                                }
                                else
                                {
                                    sidePot += win;
                                    bets[i].Bet -= win;
                                }
                        win += foldedPot + sidePot;
                        for (int i = 0; i < maxValues.Count; i++)
                        {
                            var p = lastPlayers.First(mp => mp.Agent.GetName() == maxValues[i].Name);
                            p.Stack += win / maxValues.Count;
                            log.Add(GameState.ENDINFO, p.Agent.GetName() + " won " + (win / maxValues.Count) + " chips with " + Hand.DescriptionFromHandValueInternal(maxValues[i].HandValue) + ".");
                        }
                        pot -= win;
                        if (win % maxValues.Count != 0)
                        {
                            int mod = win % maxValues.Count;
                            int button = tInfo.Button;
                            EmulatorPlayer p = null;
                            var winnigPlayers = maxValues.Select(v => v.Name).ToList();
                            while (!winnigPlayers.Contains((p = players[GetNextIndexInGame(button)]).Agent.GetName()))
                                button = GetNextIndexInGame(button);
                            p.Stack += mod;
                        }
                        bets.Remove(maxValues[0]);
                    }
                }
                for (int i = 0; i < allPlayers.Count; i++)
                    if (allPlayers[i].Stack == 0)
                        allPlayers[i].InGame = false;
                    else
                        allPlayers[i].Folded = false;
                if (allPlayers.Sum(p => p.Stack) != 1500)
                {

                }
                return true;
            }
            return false;
        }

        public int GetNotFoldedAndNotAllInInGamePlayers()
        {
            return players.Count(p => p.Folded == false && p.Stack > 0);
        }

        private int GetIndexPlayerByName(string name)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Agent.GetName() == name)
                    return i;
            }
            return 0;
        }

        private int GetNextIndexInGame(int index)
        {
            return (index + 1) % players.Count;
        }

        private int GetNextIndexInGame(string name)
        {
            return GetNextIndexInGame(GetIndexPlayerByName(name));
        }
    }
    class Bets
    {
        public Bets() { }
        public int Bet;
        public uint HandValue;
        public string Name;
    }
}
