/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;

namespace Piot.Hazy
{
    public class LatencySimulator
    {
        private long latencyInMs;
        private long lastUpdate;
        private long nextSpikeTime;
        private int nextSpikeDuration;
        private IRandom random;
        
        public LatencySimulator(int latencyInMs, Milliseconds now, IRandom random)
        {
            this.latencyInMs = latencyInMs;
            this.random = random;
            lastUpdate = now.ms;
            nextSpikeTime = now.ms + 200 + random.Random(4000);;
        }

        public void Update(Milliseconds now)
        {
            var timePassed = now.ms - lastUpdate;
            lastUpdate = now.ms;
            
            var latencyTarget = latencyInMs;
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
                    latencyTarget += 100;
                }
            }

            var diff = latencyTarget - latencyInMs;
            var adjustment = diff * (timePassed / 1000.0f);
            latencyInMs += (long) adjustment;
        }

        public long LatencyInMs => latencyInMs;
    }
}