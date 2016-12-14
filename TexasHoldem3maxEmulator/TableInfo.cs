using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemEmulator
{
    public class TableInfo
    {
        public readonly Dictionary<string, int> Players;
        public readonly int Button;
        public readonly int SmallBlind;
        public readonly long HandId;
        public TableInfo(Dictionary<string, int> players, int button, int sb, long handId)
        {
            Players = players;
            Button = button;
            SmallBlind = sb;
            HandId = handId;
        }
    }
}
