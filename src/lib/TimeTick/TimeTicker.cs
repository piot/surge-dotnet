/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;

namespace Piot.Surge.TimeTick
{
    public sealed class TimeTicker : ITimeTicker
    {
        private readonly ILog log;
        private readonly Action Tick;
        private long deltaTimeMs;
        private TimeMs lastTick;
#if DEBUG
        private TimeMs lastUpdateTime;
#endif

        public TimeTicker(TimeMs now, Action action, FixedDeltaTimeMs deltaTimeMs, ILog log)
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

        public FixedDeltaTimeMs DeltaTime
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

        public TimeMs Now { get; private set; }

        public void Update(TimeMs now)
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
            switch (iterationCount)
            {
                case <= 0:
                    return;
                case > 4:
                    iterationCount = 4;
                    break;
            }

            lastTick = new(lastTick.ms + iterationCount * deltaTimeMs);

            Now = lastTick;
            for (var i = 0; i < iterationCount; i++)
            {
                Tick();
            }
        }
    }
}