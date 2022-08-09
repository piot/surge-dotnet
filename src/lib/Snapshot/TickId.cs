/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Snapshot
{
    /// <summary>
    ///     A tick Id. Always increasing by one and consecutive for each simulation tick.
    ///     Usually the session is starting at tickId zero.
    /// </summary>
    public struct TickId
    {
        public uint tickId;

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