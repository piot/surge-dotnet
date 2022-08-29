/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    ///     Holds reusable packs (serialized values) for all the entities that has changed from one tick to the next.
    ///     The values are stored in this container, so they can be fetched again if a delta snapshots needs to be resent.
    ///     It also serves as a small optimization for the current delta snapshot, the field values doesn't have to be
    ///     re-serialized for each client.
    /// </summary>
    public class DeltaSnapshotPackContainer
    {
        public ReadOnlyMemory<byte> SerializedPack { get; }

        public TickId TickId { get; }

        public DeltaSnapshotPackContainer(TickId tickId, ReadOnlySpan<byte> serializedPack)
        {
            TickId = tickId;
            SerializedPack = serializedPack.ToArray();
        }
        public override string ToString()
        {
            return $"[DeltaSnapshotPackContainer {TickId} {SerializedPack.Length}]";
        }
    }
}