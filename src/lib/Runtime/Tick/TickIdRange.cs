/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Tick
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

        public bool Equals(TickIdRange other)
        {
            return other.startTickId == startTickId && other.lastTickId == lastTickId;
        }

        public override bool Equals(object? obj)
        {
            return obj is not null && base.Equals((TickIdRange)obj);
        }

        public static bool operator !=(TickIdRange a, TickIdRange b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(TickIdRange a, TickIdRange b)
        {
            return a.Equals(b);
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

        public bool CanAppend(TickIdRange other)
        {
            return other.Last > Last && other.startTickId <= Last.Next;
        }

        public bool CanBeFollowing(TickId otherTickId)
        {
            return Last > otherTickId && startTickId <= otherTickId.Next;
        }

        public bool IsOverlappingAndMerged(TickId tickId)
        {
            if (lastTickId <= tickId)
            {
                throw new ArgumentOutOfRangeException(nameof(tickId),
                    "can not answer if it is overlapped and merging, since it is in an illegal range");
            }

            var containsTickAlreadyKnown = startTickId <= tickId;
            var isMergedRange = Length > 1;
            return containsTickAlreadyKnown && isMergedRange;
        }

        public override string ToString()
        {
            return $"[tickIdRange {startTickId} {lastTickId}]";
        }

        public TickId lastTickId;
        public TickId startTickId;
    }
}