/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public readonly struct TimeMs
    {
        public readonly long ms;

        public TimeMs(long ms)
        {
            this.ms = ms;
        }

        public override string ToString()
        {
            return $"[monotonic-ms: {ms}]";
        }

        public bool IsBeforeOrAt(TimeMs other)
        {
            return ms <= other.ms;
        }

        public static TimeMs operator +(TimeMs timeMs, FixedDeltaTimeMs deltaTimeMs)
        {
            return new(timeMs.ms + deltaTimeMs.ms);
        }

        public static TimeMs operator -(TimeMs timeMs, FixedDeltaTimeMs deltaTimeMs)
        {
            return new(timeMs.ms - deltaTimeMs.ms);
        }
    }
}