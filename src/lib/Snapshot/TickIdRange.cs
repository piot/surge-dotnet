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
        public TickIdRange(TickId containsFromTickId, TickId tickId)
        {
            if (containsFromTickId.tickId > tickId.tickId)
            {
                throw new ArgumentOutOfRangeException($"snapshotId {containsFromTickId} {tickId}");
            }

            this.containsFromTickId = containsFromTickId;
            this.tickId = tickId;
        }

        public uint Length => tickId.tickId - containsFromTickId.tickId + 1;

        public bool IsImmediateFollowing(TickIdRange other)
        {
            return other.tickId.tickId + 1 == containsFromTickId.tickId;
        }

        public override string ToString()
        {
            return $"[tickIdRange {containsFromTickId} {tickId}]";
        }

        public TickId tickId;
        public TickId containsFromTickId;
    }
}