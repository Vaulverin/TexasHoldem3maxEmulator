using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using HoldemHand;
using TexasHoldemEmulator.Agents;

namespace TexasHoldemEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Texas Holdem Nolimit Emulator");
            Console.WriteLine("Choose players count (2 or 3):");
            int playersCount = 0;
            while(playersCount == 0)
            {
                var k = Console.ReadKey();
                switch(k.KeyChar)
                {
                    case '2':
                        playersCount = 2;
                        break;
                    case '3':
                        playersCount = 3;
                        break;
                }
                Console.WriteLine();
            }
            Console.WriteLine("This players availible:");
            Console.WriteLine(" - 1. Stupid bot");
            Console.WriteLine(" - 2. Tight bot");
            Console.WriteLine(" - 3. Player");
            List<string> agents = new List<string>();
            bool showLog = false;
            for (int i = 0; i < playersCount; i++)
            {
                Console.WriteLine("Choose player {0}:", i + 1);
                int aCount = agents.Count;
                while(agents.Count == aCount)
                {
                    var k = Console.ReadKey();
                    switch (k.KeyChar)
                    {
                        case '1': agents.Add("StupidBot");
                            break;
                        case '2': agents.Add("TightBot");
                            break;
                        case '3':
                            showLog = true;
                            agents.Add("Player");
                            break;
                        case '4':
                            agents.Add("SuperBot");
                            break;
                    }
                    Console.WriteLine();
                }
            }
            int tablesCount = 1;
            if (showLog == false)
            {
                Console.WriteLine("Show game log? (y/n)");
                string response = "";
                while(response == "")
                {
                    var k = Console.ReadKey();
                    switch(k.KeyChar)
                    {
                        case 'y': response = "y";
                            break;
                        case 'n': response = "n";
                            break;
                    }
                    Console.WriteLine();
                }
                if (response == "y")
                    showLog = true;
                else
                {
                    Console.WriteLine("Set tables count to play:");
                    while (!int.TryParse(Console.ReadLine(),out tablesCount))
                        Console.WriteLine("Wrong number. Try again:");
                }
            }
            
            int handsCount = 1000;
            Parallel.For(0, tablesCount, (int i) => 
            {
                List<EmulatorPlayer> players = new List<EmulatorPlayer>();
                agents.ForEach(a => players.Add(new EmulatorPlayer(GetNewAgentByName(a), 1500 / agents.Count)));
                StackPlayers sPlayers = new StackPlayers(players);
                int smallBlind = 10;
                for (int handNum = 0; handNum < handsCount; handNum++)
                {
                    if (sPlayers.Count < 2)
                    {
                        if (showLog)
                            Console.WriteLine("Table is over");
                        break;
                    }
                    // Перемещаем баттон, если не первая раздача
                    if (handNum != 0)
                        sPlayers.MoveButton();
                    else
                        sPlayers.SetRandomButton();
                    long handId = i * handsCount + handNum;
                    // Повышаем блайнды
                    if (handNum != 0 && handNum % 100 == 0)
                        smallBlind += 10;
                    // Создаем инфо стола
                    TableInfo tInfo = new TableInfo(sPlayers.Players, sPlayers.Button, smallBlind, handId);
                    var log = HandRound(sPlayers, tInfo, showLog);
                    
                }
                for (int j = 0; j < sPlayers.Count; j++)
                    Console.WriteLine(sPlayers[j].Agent.GetName() + " - " + sPlayers[j].Stack);
            });
            Console.WriteLine("All done.");
            Console.ReadKey();
        }

        private static IAgent GetNewAgentByName(string name)
        {
            switch (name)
            {
                case "StupidBot":
                    return new StupidBot();
                case "TightBot":
                    return new TightBot();
                case "Player":
                    return new Player();
            }
            throw new ArgumentException(name + " - agent doesn't ecxists!");
        }
        
        private static TableLog HandRound(StackPlayers sPlayers, TableInfo tInfo, bool showLog)
        {
            ulong dead = 0UL;
            ulong board = 0UL;
            TableLog log = new TableLog(showLog);
            GameState state = GameState.STARTINFO;
            log.Add(state,
                "Hand ID - " + tInfo.HandId + ", small blind - " + tInfo.SmallBlind + ", players count - " + sPlayers.Count + ", button seats on - " + (tInfo.Button + 1));
            for (int p = 0; p < sPlayers.Count; p++)
                    log.Add(state,
                        "Seat " + (p + 1) + ": " + sPlayers[p].Agent.GetName() + " has stack - " + sPlayers[p].Stack);
            #region Streets
            List<Action> streets = new List<Action>
                    {
                        () =>
                        {
                            // Раздаем карты
                            for (int p = 0; p < sPlayers.Count; p++)
                            {
                                sPlayers[p].Hand = Hand.RandomHand(dead, 2);
                                dead |= sPlayers[p].Hand;
                                if (sPlayers[p].Agent.GetType() == typeof(Player))
                                {
                                    log.Add(state, sPlayers[p].Agent.GetName() + " pocket cards are " + Hand.MaskToString(sPlayers[p].Hand));

                                }
                            }
                            state = GameState.PREFLOP;
                            log.Add(state, "*** " + state.ToString() + " ***");
                        },
                        () =>
                        {
                            ulong flop = Hand.RandomHand(dead, 3);
                            board = flop;
                            dead |= flop;
                            state = GameState.FLOP;
                            log.Add(state, "*** FLOP *** [" + Hand.MaskToString(flop) + "]");
                        },
                        () =>
                        {
                            ulong turn = Hand.RandomHand(dead, 1);
                            state = GameState.TURN;
                            log.Add(state, "*** TURN *** [" + Hand.MaskToString(board) + "]  [" + Hand.MaskToString(turn) + "]");
                            board |= turn;
                            dead |= turn;
                        },
                        () =>
                        {
                            ulong river = Hand.RandomHand(dead, 1);
                            state = GameState.RIVER;
                            log.Add(state, "*** RIVER *** [" + Hand.MaskToString(board) + "]  [" + Hand.MaskToString(river) + "]");
                            board |= river;
                        }
                    };
            #endregion
            // Создаем начальную ситуацию с пустым столом
            BoardSituation currentSituation = null;
            foreach (var street in streets)
            {
                street();
                currentSituation = new BoardSituation(currentSituation, board, "", -1);
                EmulatorPlayer player;
                while (player = sPlayers.GetNextPlayer(currentSituation, tInfo))
                {
                    int decision = -1;
                    // Если нужно ставить блайды - ставим, иначе запрашиваем ставку у агента
                    if (currentSituation.Deep == 0)
                        decision = tInfo.SmallBlind;
                    else if (currentSituation.Deep == 1)
                        decision = tInfo.SmallBlind * 2;
                    else
                        decision = player.Agent.GetDecision(currentSituation, tInfo);
                    //Проверяем верность ставки
                    int currentBet = currentSituation.GetPlayerCurrentBet(player.Agent.GetName());
                    if (decision >= 0)
                        if (decision + currentBet < currentSituation.MaxBet ||
                            sPlayers.GetNotFoldedAndNotAllInInGamePlayers() == 1 
                            || currentSituation.RaiseCount == 3)
                            decision = currentSituation.MaxBet - currentBet;
                    if (decision > player.Stack)
                        decision = player.Stack;
                    else if (decision < -1)
                        decision = -1;
                    // Добавляем ситуацию в дерево
                    currentSituation = new BoardSituation(currentSituation, board, player.Agent.GetName(), decision);
                    log.Add(state, player.Agent.GetName() + ": " + currentSituation.GetActionString(tInfo));
                }
                if (sPlayers.IsGameOver(currentSituation, tInfo, ref log))
                    break;
            }
            log.Add(GameState.ENDINFO, "Game Over");
            return log;
        }

    }
}
