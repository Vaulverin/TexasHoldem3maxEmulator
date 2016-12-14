using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace HoldemHand
{
    public struct HandSpectrumCell
    {
        public string CellSymbol;
        public double Weight;
    }
    struct Results
    {
        public long win, ties, ties2, total;
    }

    public static class HandEquity
    {
        static bool IsListDead(ulong[] list, ulong dead)
        {
            for (int i = 0; i < list.Length; i++)
                if (Hand.BitCount(list[i] & dead) == 0)
                    return false;
            return true;
        }

        delegate Results CalculateOddsDelegate(ulong[] heroPockets, CellsArray opp1Sp, CellsArray opp2Sp = null);
        
        static Results CalculateOdds(ulong[] heroPockets, CellsArray opp1Sp, CellsArray opp2Sp = null)
        {
            Results results = new Results();
            for(int i = 0; i < 1000; i++)
            {
                ulong pocketmask = Hand.RandomHand(heroPockets, 0UL, 2);
                ulong opp1 = Hand.RandomHand(opp1Sp.HandsList, pocketmask, 2);
                ulong dead = opp1 | pocketmask;
                ulong opp2 = 0UL;
                if (opp2Sp != null)
                {
                    opp2 = Hand.RandomHand(opp2Sp.HandsList, dead, 2);
                    dead |= opp2;
                }
                ulong board = Hand.RandomHand(0UL, dead, 5);
                uint playerHandval = Hand.Evaluate(pocketmask | board, 7);
                uint oppHandval = Hand.Evaluate(opp1 | board, 7);
                int result = -1;

                if (playerHandval > oppHandval)
                    result = 1;
                else if (playerHandval == oppHandval)
                    result = 0;
                
                if (opp2 != 0UL && result != -1)
                {
                    uint opp2Handval = Hand.Evaluate(opp2 | board, 7);
                    if (playerHandval > opp2Handval)
                        result += 1;
                    else if (playerHandval == opp2Handval)
                        result += 0;
                    else
                        result = -1;
                }
                if (result == 2)
                    results.win++;
                else if (result == 1)
                    results.ties++;
                else if (result == 0)
                    results.ties2++;
                
                results.total++;
            }
            return results;
        }

        public static double CellVsSpectrums(string heroCell, HandSpectrumCell[] opp1Spectrum, HandSpectrumCell[] opp2Spectrum = null)
        {
            if(heroCell.Length == 2 && opp2Spectrum != null)
            {
                bool holeLine1 = opp1Spectrum.Count(e => e.CellSymbol.Contains(heroCell[0])) == opp1Spectrum.Length;
                bool holeLine2 = opp2Spectrum.Count(e => e.CellSymbol.Contains(heroCell[0])) == opp1Spectrum.Length;
                bool hasSameCell1 = opp1Spectrum.Count(e => e.CellSymbol == heroCell) != 0;
                bool hasSameCell2 = opp2Spectrum.Count(e => e.CellSymbol == heroCell) != 0;
                if (hasSameCell1 && holeLine2)
                    opp1Spectrum = opp1Spectrum.Where(e => e.CellSymbol != heroCell).ToArray();
                if (hasSameCell2 && holeLine1)
                    opp2Spectrum = opp2Spectrum.Where(e => e.CellSymbol != heroCell).ToArray();
            }

            IAsyncResult[] results = new IAsyncResult[1000];
            CalculateOddsDelegate d = new CalculateOddsDelegate(CalculateOdds);
            long wins = 0, ties = 0, ties2 = 0, total = 0;
            ulong[] heroPockets = PocketHands.Query(heroCell);
            var opp1Sp = new CellsArray(opp1Spectrum);
            CellsArray opp2Sp = null;
            if (opp2Spectrum != null)
                opp2Sp = new CellsArray(opp2Spectrum);
            for(int i = 0; i < results.Length; i++)
            {
                results[i] = d.BeginInvoke(heroPockets, opp1Sp, opp2Sp, null, null);
            }
            var situations = new Dictionary<string, long>();
            for (int i = 0; i < results.Length; i++)
            {
                Results r = d.EndInvoke(results[i]);
                wins += r.win;
                ties += r.ties;
                ties2 += r.ties2;
                total += r.total;
            }
            return ((double)(wins + ties / 2 + ties2 / 3) / total * 100);
        }
        
    }

    class CellsArray
    {
        private Dictionary<double, ulong[]> Cells = new Dictionary<double, ulong[]>();
        public ulong[] HandsList;

        public CellsArray(HandSpectrumCell[] spectrum)
        {
            double weightSum = spectrum.Sum(c => c.Weight);
            double lastWeight = 0;
            var hList = new List<ulong>();
            for(int i = 0; i < spectrum.Length; i++)
            {
                double currentWeight = spectrum[i].Weight / weightSum;
                lastWeight += currentWeight;
                var cell = PocketHands.Query(spectrum[i].CellSymbol);
                Cells.Add(lastWeight, cell);
                for (int j = 0; j < currentWeight * 100; j++)
                    hList.Add(Hand.RandomHand(cell, 0UL, 2));
            }
            HandsList = hList.ToArray();
        }

        public ulong[] GetRandomCell()
        {
            double index = StrongRandom.NextDouble();
            for(int i = 0; i < Cells.Count; i++)
            {
                if (Cells.Keys.ElementAt(i) > index)
                    return Cells.Values.ElementAt(i);
            }
            return Cells.ElementAt(0).Value;
        }

    }
    static class StrongRandom
    {
        [ThreadStatic]
        private static Random _random;

        public static int NextInt(int inclusiveLowerBound, int exclusiveUpperBound)
        {
            if (_random == null)
            {
                var cryptoResult = new byte[4];
                new RNGCryptoServiceProvider().GetBytes(cryptoResult);

                int seed = BitConverter.ToInt32(cryptoResult, 0);

                _random = new Random(seed);
            }
            return _random.Next(inclusiveLowerBound, exclusiveUpperBound);
        }

        public static double NextDouble()
        {
            if (_random == null)
            {
                var cryptoResult = new byte[4];
                new RNGCryptoServiceProvider().GetBytes(cryptoResult);

                int seed = BitConverter.ToInt32(cryptoResult, 0);

                _random = new Random(seed);
            }
            return _random.NextDouble();
        }
    }
}
