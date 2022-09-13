/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;

namespace Piot.Surge.TimeTick
{
    public class TimeTicker : ITimeTicker
    {
        private readonly ILog log;
        private readonly Action Tick;
        private long deltaTimeMs;
        private Milliseconds lastTick;
#if DEBUG
        private Milliseconds lastUpdateTime;
#endif

        public TimeTicker(Milliseconds now, Action action, Milliseconds deltaTimeMs, ILog log)
        {
            if (deltaTimeMs.ms <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeMs), deltaTimeMs.ms,
                    $"illegal monotonic time <= 0 {deltaTimeMs}");
            }

            Tick = action;
            lastTick = now;
            this.deltaTimeMs = deltaTimeMs.ms;
            this.log = log;
#if DEBUG
            lastUpdateTime = now;
#endif
        }

        public Milliseconds DeltaTime
        {
            set
            {
                if (value.ms < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(deltaTimeMs), deltaTimeMs,
                        $"illegal monotonic time <= 0 {deltaTimeMs}");
                }

                deltaTimeMs = value.ms;
            }
        }

        public Milliseconds Now { get; private set; }

        public void Update(Milliseconds now)
        {
#if DEBUG
            if (now.ms < lastUpdateTime.ms)
            {
                throw new ArgumentOutOfRangeException(nameof(now), now.ms,
                    $"monotonic time can only stand still or move forward ${now} ${lastTick}");
            }

            lastUpdateTime = now;

            if (now.ms < lastTick.ms)
            {
                throw new ArgumentOutOfRangeException(nameof(now), now.ms,
                    $"monotonic time can only stand still or move forward ${now} ${lastTick}");
            }

#endif
            if (deltaTimeMs == 0)
            {
                lastTick = now;
                return;
            }

            var iterationCount = (now.ms - lastTick.ms) / deltaTimeMs;
            if (iterationCount <= 0)
            {
                return;
            }

            if (iterationCount > 4)
            {
                iterationCount = 4;
            }

            lastTick = new Milliseconds(lastTick.ms + iterationCount * deltaTimeMs);

            Now = lastTick;
            for (var i = 0; i < iterationCount; i++)
            {
                Tick();
            }
        }
    }
}