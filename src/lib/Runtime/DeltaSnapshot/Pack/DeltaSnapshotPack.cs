/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.Pack
{
    /// <summary>
    ///     Holds a complete delta snapshot serialized pack deltaSnapshotPackPayload.
    ///     Holds reusable packs (serialized values) for all the entities that has changed from one tick to the next.
    ///     The values are stored in this container, so they can be fetched again if a delta snapshots needs to be resent.
    ///     It also serves as a small optimization for the current delta snapshot, the field values doesn't have to be
    ///     re-serialized for each client.
    /// </summary>
    public sealed class DeltaSnapshotPack
    {
        public ReadOnlyMemory<byte> payload;
        public TickIdRange tickIdRange;

        public DeltaSnapshotPack(TickIdRange tickIdRange, ReadOnlySpan<byte> payload,
            SnapshotType snapshotType)
        {
            this.tickIdRange = tickIdRange;
            this.payload = payload.ToArray();
            SnapshotType = snapshotType;
        }

        public TickIdRange TickIdRange => tickIdRange;

        public SnapshotType SnapshotType { get; }

        static string OctetArrayToString(ReadOnlySpan<byte> octetArray)
        {
            return BitConverter.ToString(octetArray.ToArray()).Replace("-", "");
        }

        public override string ToString()
        {
            var truncate = payload.Length > 8 ? 8 : payload.Length;
            var wasTruncated = truncate < payload.Length;
            var octetString = OctetArrayToString(payload.Span.Slice(0, truncate));
            if (wasTruncated)
            {
                octetString += "...";
            }

            return $"[snapshotDeltaPack tickId: {tickIdRange} type:{SnapshotType}  payload length: {payload.Length} octets:{octetString}]";
        }
    }
}