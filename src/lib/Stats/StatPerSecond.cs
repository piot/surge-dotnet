using System.Runtime.CompilerServices;
using Piot.MonotonicTime;

namespace Piot.Stats
{
    public class StatOverTime
    {
        private Stats stats;
        long total;
        private uint count;
        private Milliseconds lastTime;
        private readonly Milliseconds minimumAverageTime;
        private int min;
        private int max;
        
        public StatOverTime(Milliseconds now, Milliseconds minimumAverageTime)
        {
            Reset(now);
            this.minimumAverageTime = minimumAverageTime;
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
        }

        void Reset(Milliseconds now)
        {
            min = int.MaxValue;
            max = int.MinValue;
            count = 0;
            total = 0;
            lastTime = now;
        }
        
        public Stats Stats => stats;

        public void Update(Milliseconds now)
        {
            if (now.ms < lastTime.ms + minimumAverageTime.ms)
            {
                return;
            }

            var deltaMilliseconds = now.ms - lastTime.ms;
            stats.average = (int)(total * 1000 / deltaMilliseconds);
            stats.min = min;
            stats.max = max;
            stats.count = count;

            Reset(now);
        }
    }
}