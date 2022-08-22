/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;

namespace Piot.Stats
{
    public class StatCountThreshold
    {
        private readonly uint countThreshold;
        private uint count;
        private int max;
        private int min;
        private Stat stat;
        private long total;

        public StatCountThreshold(uint countThreshold)
        {
            this.countThreshold = countThreshold;
            Reset();
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

            if (count < countThreshold)
            {
                return;
            }

            stat.average = (int)(total / count);
            stat.min = min;
            stat.max = max;
            stat.count = count;

            Reset();
        }

        private void Reset()
        {
            min = int.MaxValue;
            max = int.MinValue;
            count = 0;
            total = 0;
        }
    }
}