/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;

namespace Piot.Surge.TimeTicker
{
    public class TimeTicker : ITimeTicker
    {
        private long deltaTimeMs;
        private readonly ILog log;
        private readonly Action Tick;
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
            lastTick.ms = now.ms;
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
                if (value.ms <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(deltaTimeMs), deltaTimeMs,
                        $"illegal monotonic time <= 0 {deltaTimeMs}");
                }

                deltaTimeMs = value.ms;
            }
        }


        public void Update(Milliseconds now)
        {
#if DEBUG
            if (now.ms < lastUpdateTime.ms)
            {
                throw new ArgumentOutOfRangeException(nameof(now), now.ms,
                    $"monotonic time can only stand still or move forward ${now} ${lastTick}");
            }

            lastUpdateTime.ms = now.ms;

#endif
            var iterationCount = (now.ms - lastTick.ms) / deltaTimeMs;
            if (iterationCount <= 0)
            {
                return;
            }

            lastTick.ms += iterationCount * deltaTimeMs;

            for (var i = 0; i < iterationCount; i++)
            {
                log.Debug("Tick() {Time} {Iteration}", now.ms, i);
                Tick();
            }
        }
    }
}