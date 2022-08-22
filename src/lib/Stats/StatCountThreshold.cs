using System.Runtime.CompilerServices;

namespace Piot.Stats
{
    public class Stat
    {
        private Stats stats;
        long total;
        private uint count;
        private int min;
        private int max;
        private readonly uint countThreshold;
        
        public Stat(uint countThreshold)
        {
            this.countThreshold = countThreshold;
            Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int a)
        {
            total += a;
            count++;

            if (a < min)
            {
                min = a;
            }

            if (a > max)
            {
                max = a;
            }

            if (count >= countThreshold)
            {
                stats.average = (int)(total / count);
                stats.min = min;
                stats.max = max;
                stats.count = count;
                Reset();
            }
        }

        void Reset()
        {
            min = int.MaxValue;
            max = int.MinValue;
            count = 0;
            total = 0;
        }
        
        public Stats Stats => stats;
    }
}