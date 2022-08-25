/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Random;

namespace Piot.Hazy
{
    /// <summary>
    ///     Simulates the latency that can be encountered on the Internet.
    ///     Currently provides simplistic latency "jitter" (inter-packet delay variance)
    ///     and short latency bursts.
    /// </summary>
    public class LatencySimulator
    {
        private readonly ILog log;
        private readonly IRandom random;
        private bool isSpiking;
        private long lastUpdate;
        private long latencyInMs;
        private long longTermTargetInMs;

        private long maxLatency;
        private long minLatency;
        private long nextLongTermTargetTime;
        private int nextSpikeDuration;
        private long nextSpikeTime;
        private long savedLatencyBeforeSpike;
        private long targetLatencyInMs;

        public LatencySimulator(int minimumLatency, int maximumLatency, Milliseconds now, IRandom random, ILog log)
        {
            this.log = log;
            this.random = random;
            lastUpdate = now.ms;
            nextSpikeTime = now.ms + 200 + random.Random(4000);
            nextSpikeDuration = 100;
            longTermTargetInMs = targetLatencyInMs;
            nextLongTermTargetTime = now.ms;
            SetLatencyRange(minimumLatency, maximumLatency);
            latencyInMs = targetLatencyInMs;
        }

        public Milliseconds LatencyInMs => new(latencyInMs);

        public void SetLatencyRange(int minimumLatency, int maximumLatency)
        {
            if (minLatency > maxLatency)
            {
                throw new Exception("illegal min or max latency");
            }

            minLatency = minimumLatency;
            maxLatency = maximumLatency;
            targetLatencyInMs = minimumLatency + (maximumLatency - minimumLatency) / 2;
        }

        /// <summary>
        ///     Update latency given the monotonic time <paramref name="now" />.
        ///     TODO: Needs a more sophisticated solution
        /// </summary>
        /// <param name="now"></param>
        public void Update(Milliseconds now)
        {
            var timePassed = now.ms - lastUpdate;
            if (timePassed < 10)
            {
                return;
            }

            lastUpdate = now.ms;

            if (now.ms >= nextLongTermTargetTime)
            {
                nextLongTermTargetTime = now.ms + random.Random(1000) + 10000;
                var range = maxLatency - minLatency;
                var upwards = Math.Clamp(maxLatency - latencyInMs, 0, range);
                var downwards = Math.Clamp(latencyInMs - minLatency, 0, range);
                var delta = random.Random((int)upwards) - random.Random((int)downwards);
                longTermTargetInMs = latencyInMs + delta;
                longTermTargetInMs = Math.Clamp(longTermTargetInMs, minLatency, maxLatency);
                log.DebugLowLevel("setting new latency target {TargetInMs}", longTermTargetInMs);
            }

            var jitter = 4 - random.Random(8);
            latencyInMs += jitter;

            if (now.ms >= nextSpikeTime)
            {
                if (!isSpiking)
                {
                    isSpiking = true;
                    savedLatencyBeforeSpike = latencyInMs;
                    log.DebugLowLevel("Spiking started!");
                }

                var timeInSpike = now.ms - nextSpikeTime;
                if (timeInSpike > nextSpikeDuration)
                {
                    latencyInMs = savedLatencyBeforeSpike;
                    isSpiking = false;
                    log.DebugLowLevel("Spiking over!");
                    nextSpikeDuration = 50 + random.Random(100);
                    nextSpikeTime = now.ms + 8000 + random.Random(4000);
                }
                else
                {
                    latencyInMs = maxLatency - 8;
                    var spikeJitter = 8 - random.Random(16);
                    latencyInMs += spikeJitter;
                    return;
                }
            }

            var diff = longTermTargetInMs - latencyInMs;
            var adjustment = diff * (timePassed / 100.0f);
            latencyInMs += (long)adjustment;
            latencyInMs = Math.Clamp(latencyInMs, minLatency, maxLatency);
        }
    }
}