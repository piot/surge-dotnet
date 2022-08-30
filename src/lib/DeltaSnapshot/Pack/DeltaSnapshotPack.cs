/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.Pack
{
    /// <summary>
    ///     Holds a complete delta snapshot serialized pack payload.
    ///     Holds reusable packs (serialized values) for all the entities that has changed from one tick to the next.
    ///     The values are stored in this container, so they can be fetched again if a delta snapshots needs to be resent.
    ///     It also serves as a small optimization for the current delta snapshot, the field values doesn't have to be
    ///     re-serialized for each client.
    /// </summary>
    public class DeltaSnapshotPack
    {
        public ReadOnlyMemory<byte> payload;
        public TickIdRange tickIdRange;

        public DeltaSnapshotPack(TickIdRange tickIdRange, ReadOnlySpan<byte> payload)
        {
            this.tickIdRange = tickIdRange;
            this.payload = payload.ToArray();
        }

        public TickIdRange TickIdRange => tickIdRange;

        public override string ToString()
        {
            return $"[snapshotDeltaPack tickId: {tickIdRange} payload length: {payload.Length}]";
        }
    }
}