/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    /// <summary>
    ///     The 16 lower bits of a monotonic <see cref="TimeMs" />.
    /// </summary>
    public readonly struct MonotonicTimeLowerBits
    {
        public readonly ushort lowerBits;

        public MonotonicTimeLowerBits(ushort lowerBits)
        {
            this.lowerBits = lowerBits;
        }

        public override string ToString()
        {
            return $"[LowerBits {lowerBits:X4}]";
        }

        public static MonotonicTimeLowerBits FromTime(TimeMs timeMs)
        {
            return new((ushort)(timeMs.ms & 0xffff));
        }
    }
}