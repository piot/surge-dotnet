/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;

namespace Piot.Hazy
{
    /// <summary>
    /// Simulates the latency that can be encountered on the Internet.
    /// Currently provides simplistic latency "jitter" (inter-packet delay variance)
    /// and short latency bursts.
    /// </summary>
    public class LatencySimulator
    {
        private long latencyInMs;
        private long targetLatencyInMs;
        private long lastUpdate;
        private long nextSpikeTime;
        private int nextSpikeDuration;
        private readonly IRandom random;
        private readonly int minLatency;
        private readonly int maxLatency;
        
        public LatencySimulator(int minimumLatency, int maximumLatency, Milliseconds now, IRandom random)
        {
            this.random = random;
            lastUpdate = now.ms;
            nextSpikeTime = now.ms + 200 + random.Random(4000);
            nextSpikeDuration = 100;
            minLatency = minimumLatency;
            maxLatency = maximumLatency;
            targetLatencyInMs = minimumLatency + (maximumLatency - minimumLatency) / 2;
            latencyInMs = targetLatencyInMs;
        }

        /// <summary>
        /// Update latency given the monotonic time <paramref name="now"/>.
        /// TODO: Needs a more sophisticated solution
        /// </summary>
        /// <param name="now"></param>
        public void Update(Milliseconds now)
        {
            var timePassed = now.ms - lastUpdate;
            lastUpdate = now.ms;

            var diffInTarget = 10 - random.Random(20);

            targetLatencyInMs += diffInTarget;

            if (targetLatencyInMs < minLatency)
            {
                targetLatencyInMs += diffInTarget;
            }

            if (targetLatencyInMs > maxLatency)
            {
                targetLatencyInMs -= diffInTarget;
            }
            
            var jitter = 5 - random.Random(10);
            var latencyTargetThisUpdate = targetLatencyInMs + jitter;
            
            if (now.ms >= nextSpikeTime)
            {
                var timeInSpike = now.ms - nextSpikeTime;
                if (timeInSpike > nextSpikeDuration)
                {
                    nextSpikeDuration = 100 + random.Random(200);
                    nextSpikeTime = now.ms + 200 + random.Random(4000);
                }
                else
                {
                    latencyTargetThisUpdate += 100;
                }
            }

            var diff = latencyTargetThisUpdate - latencyInMs;
            var adjustment = diff * (timePassed / 1000.0f);
            latencyInMs += (long) adjustment;
        }

        public Milliseconds LatencyInMs => new (latencyInMs);
    }
}