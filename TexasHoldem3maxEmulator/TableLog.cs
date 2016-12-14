using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemEmulator
{
    public class TableLog
    {
        private Dictionary<GameState, List<string>> log = new Dictionary<GameState, List<string>>();
        private bool showLog = false;

        public TableLog(bool showLog)
        {
            this.showLog = showLog;
        }
        public void Add(GameState state, string line)
        {
            if (!log.ContainsKey(state))
                log.Add(state, new List<string>());
            log[state].Add(line);
            if (showLog)
                Console.WriteLine(line);
        }

        public bool Contains(GameState state)
        {
            return log.ContainsKey(state);
        }

        public List<string> this[GameState state]
        {
            get { return log[state]; }
        }

    }

    public enum GameState
    {
        STARTINFO,
        PREFLOP,
        FLOP,
        TURN,
        RIVER,
        ENDINFO
    }
}
