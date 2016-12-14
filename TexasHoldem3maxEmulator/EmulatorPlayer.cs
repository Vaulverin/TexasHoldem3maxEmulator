using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldemEmulator.Agents;

namespace TexasHoldemEmulator
{
    public class EmulatorPlayer
    {
        private int stack = 0;
        public int Stack { get { return stack; } set { stack = value; Agent.SetStack(stack); } }
        private ulong hand = 0UL;
        public ulong Hand { get { return hand; } set { hand = value; Agent.SetHand(hand); } }
        public readonly IAgent Agent;
        public bool InGame = true;
        public bool Folded = false;

        public EmulatorPlayer(IAgent agent, int stack)
        {
            Agent = agent;
            Stack = stack;
        }

        public static implicit operator bool(EmulatorPlayer v)
        {
            return v != null;
        }
    }
}
