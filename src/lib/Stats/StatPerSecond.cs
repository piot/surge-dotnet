/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using Piot.MonotonicTime;

namespace Piot.Stats
{
    public static class StandardFormatter
    {
        public static string Format(int value)
        {
            return $"{value}";
        }
    }

    public static class BitFormatter
    {
        public static string Format(int value)
        {
            return value switch
            {
                < 1000 => $"{value} bit",
                < 1_000_000 => $"{value / 1000.0:0.0} kbit",
                < 1_000_000_000 => $"{value / 1_000_000.0:0.#} Mbit",
                _ => $"{value / 1_000_000_000:0.#} Gbit"
            };
        }
    }

    public class StatPerSecond
    {
        private readonly Milliseconds minimumAverageTime;
        private uint count;
        private Milliseconds lastTime;
        private int max;
        private int min;
        private Stat stat;
        private long total;

        public StatPerSecond(Milliseconds now, Milliseconds minimumAverageTime, Func<int, string>? formatter = null)
        {
            stat.Formatter = formatter ?? StandardFormatter.Format;

            Reset(now);

            this.minimumAverageTime = minimumAverageTime;
        }

        public Stat Stat => stat;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int a)
        {
            total += a;
            count++; // count is technically not needed, but is nice to know how many samples the average was based on.

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