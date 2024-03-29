/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;

namespace Piot.Stats
{
    public sealed class StatCountThreshold
    {
        readonly uint countThreshold;
        uint count;
        int max;
        int min;
        Stat stat;
        long total;

        public StatCountThreshold(uint countThreshold)
        {
            stat.Formatter = StandardFormatter.Format;
            this.countThreshold = countThreshold;
            Reset();
        }


        public Stat Stat => stat;

        public bool IsReady { get; private set; }

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

            if (!IsReady)
            {
                stat.average = (int)(total / count);
                stat.min = min;
                stat.max = max;
            }


            if (count < countThreshold)
            {
                return;
            }

            stat.average = (int)(total / count);
            stat.min = min;
            stat.max = max;
            stat.count = count;
            IsReady = true;

            Reset();
        }

        void Reset()
        {
            min = int.MaxValue;
            max = int.MinValue;
            count = 0;
            total = 0;
        }
    }
}