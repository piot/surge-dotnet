/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Snapshot
{
    /// <summary>
    ///     A range of consecutive tick IDs.
    /// </summary>
    public struct TickIdRange
    {
        public TickIdRange(TickId startTickId, TickId lastTickId)
        {
            if (startTickId.tickId > lastTickId.tickId)
            {
                throw new ArgumentOutOfRangeException(nameof(startTickId), $"TickIdRange {startTickId} {lastTickId}");
            }

            this.startTickId = startTickId;
            this.lastTickId = lastTickId;
        }

        public static TickIdRange FromTickId(TickId tickId)
        {
            return new(tickId, tickId);
        }

        public bool Contains(TickIdRange other)
        {
            return other.startTickId.tickId >= startTickId.tickId && other.lastTickId.tickId <= lastTickId.tickId;
        }

        public bool Contains(TickId other)
        {
            return other.tickId >= startTickId.tickId && other.tickId <= lastTickId.tickId;
        }

        public (uint, uint) Offsets(TickIdRange other)
        {
            if (!Contains(other))
            {
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            return (other.startTickId.tickId - startTickId.tickId, other.lastTickId.tickId - startTickId.tickId);
        }

        public uint Length => lastTickId.tickId - startTickId.tickId + 1;

        public TickId Last => lastTickId;

        public bool IsImmediateFollowing(TickIdRange other)
        {
            return other.lastTickId.tickId + 1 == startTickId.tickId;
        }

        public override string ToString()
        {
            return $"[tickIdRange {startTickId} {lastTickId}]";
        }

        public TickId lastTickId;
        public TickId startTickId;
    }
}