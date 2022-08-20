using System;
using Piot.MonotonicTime;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    public static class LowerBitsToMonotonic
    {
        public static Milliseconds LowerBitsToMonotonicMs(Milliseconds now, MonotonicTimeLowerBits lowerBits)
        {
            var nowBits = (ulong)(now.ms & 0xffff);
            var receivedLowerBits = (ulong)lowerBits.lowerBits;
            var top = (ulong)now.ms & 0xffffffffffff0000;
            
            var receivedMonotonic = top | receivedLowerBits;
            if (receivedLowerBits > nowBits)
            {
                receivedMonotonic -= 0x10000;
            }

            var diff = (ulong)now.ms - receivedMonotonic;
            if (diff > 3000)
            {
                throw new Exception($"suspicious time lower bits diff {diff}");
            }

            return new Milliseconds((long)receivedMonotonic);
        }
    }
}