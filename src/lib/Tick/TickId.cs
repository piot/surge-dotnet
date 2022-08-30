/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Tick
{
    /// <summary>
    ///     A tick Id. Always increasing by one and consecutive for each simulation tick.
    ///     Usually the session is starting at lastTickId zero.
    ///     Assuming a tick delta time of 16 ms, a 32 bit value will cover a maximum session time of
    ///     about two years. The uint.MaxValue value, <c>0xffffffff</c>, is reserved and can not be
    ///     used as a normal tickId.
    /// </summary>
    public readonly struct TickId
    {
        public readonly uint tickId;

        public TickId(uint tickId)
        {
            if (tickId == uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(tickId), "max value tick id reserved");
            }

            this.tickId = tickId;
        }

        public bool IsImmediateFollowing(TickId other)
        {
            return other.tickId + 1 == tickId;
        }

        public override string ToString()
        {
            return $"{tickId}";
        }
    }
}