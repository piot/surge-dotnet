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
        private readonly long deltaTimeMs;
        private readonly ILog log;
        private Milliseconds lastTick;
        private readonly Action Tick;

        public TimeTicker(Milliseconds now, Action action, Milliseconds deltaTimeMs, ILog log)
        {
            if (deltaTimeMs.ms <= 0)
            {
                throw new ArgumentException($"illegal monotonic time <= 0 {deltaTimeMs}", nameof(deltaTimeMs));
            }

            Tick = action;
            lastTick.ms = now.ms;
            this.deltaTimeMs = deltaTimeMs.ms;
            this.log = log;
        }

        public void Update(Milliseconds now)
        {
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