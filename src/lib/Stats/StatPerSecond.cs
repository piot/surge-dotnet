/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;
using Piot.MonotonicTime;

namespace Piot.Stats
{
    public class StatPerSecond
    {
        private readonly Milliseconds minimumAverageTime;
        private uint count;
        private Milliseconds lastTime;
        private int max;
        private int min;
        private Stat stat;
        private long total;

        public StatPerSecond(Milliseconds now, Milliseconds minimumAverageTime)
        {
            Reset(now);
            this.minimumAverageTime = minimumAverageTime;
        }

        public Stat Stat => stat;

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

        private void Reset(Milliseconds now)
        {
            min = int.MaxValue;
            max = int.MinValue;
            count = 0;
            total = 0;
            lastTime = now;
        }

        public void Update(Milliseconds now)
        {
            if (now.ms < lastTime.ms + minimumAverageTime.ms)
            {
                return;
            }

            var deltaMilliseconds = now.ms - lastTime.ms;
            stat.average = (int)(total * 1000 / deltaMilliseconds);
            stat.min = min;
            stat.max = max;
            stat.count = count;

            Reset(now);
        }
    }
}