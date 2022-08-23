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

    public static class StandardFormatterPerSecond
    {
        public static string Format(int value)
        {
            return $"{value}/s";
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

    public static class BitsPerSecondFormatter
    {
        public static string Format(int value)
        {
            return $"{BitFormatter.Format(value)}/s";
        }
    }

    public class StatPerSecond
    {
        private readonly Milliseconds minimumAverageTime;
        private uint averageCount;
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
        }

        private void Reset(Milliseconds now)
        {
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

            var a = (int)(total * 1000 / deltaMilliseconds);
            if (a < min)
            {
                min = a;
            }

            if (a > max)
            {
                max = a;
            }

            stat.average = a;
            stat.min = min;
            stat.max = max;
            stat.count = count;

            averageCount++;
            if (averageCount > 5)
            {
                min = int.MaxValue;
                max = int.MinValue;
                averageCount = 0;
            }

            Reset(now);
        }
    }
}