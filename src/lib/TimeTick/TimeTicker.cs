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
        readonly ILog log;
        readonly Action Tick;
        long deltaTimeMs;
        TimeMs lastTick;
#if DEBUG
        TimeMs lastUpdateTime;
#endif

        public TimeTicker(TimeMs now, Action action, FixedDeltaTimeMs deltaTimeMs, ILog log)
        {
            if (deltaTimeMs.ms <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeMs), deltaTimeMs.ms,
                    $"illegal fixed delta time <= 0 {deltaTimeMs}");
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
            if (iterationCount > 30)
            {
                throw new($"Time is so much in the future, that we can not keep up {now.ms} vs {lastTick.ms}");
            }

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

        public void Reset(TimeMs now)
        {
            lastUpdateTime = now;
            lastTick = now;
        }
    }
}