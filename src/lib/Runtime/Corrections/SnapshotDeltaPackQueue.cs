/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.Corrections.Serialization;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections
{
    public sealed class SnapshotDeltaPackQueue : ISnapshotDeltaPackQueue, IOctetSerializable
    {
        readonly Queue<DeltaSnapshotPack> packs = new();

        TickIdRange lastInsertedTickIdRange;

        uint wantsTickIdValue = 1;

        public TickId WantsTickId => new(wantsTickIdValue);

        public bool LastInsertedIsMergedSnapshot => HasBeenInitialized && lastInsertedTickIdRange.Length > 1;
        public TickIdRange LastInsertedTickIdRange => lastInsertedTickIdRange;

        public bool HasBeenInitialized { get; private set; }

        public void Deserialize(IOctetReader reader)
        {
            var count = reader.ReadUInt8();
            var queue = new Queue<DeltaSnapshotPack>(count);
            for (var i = 0; i < count; ++i)
            {
                var newItem = DeltaSnapshotPackInternalStateReader.Read(reader);
                queue.Enqueue(newItem);
            }

            lastInsertedTickIdRange = TickIdRangeReader.Read(reader);
            wantsTickIdValue = TickIdReader.Read(reader).tickId;
            HasBeenInitialized = reader.ReadUInt8() != 0;
        }

        public void Serialize(IOctetWriter writer)
        {
            writer.WriteUInt8((byte)packs.Count);
            foreach (var packItem in packs)
            {
                DeltaSnapshotPackInternalStateWriter.Write(packItem, writer);
            }

            TickIdRangeWriter.Write(writer, lastInsertedTickIdRange);
            TickIdWriter.Write(writer, WantsTickId);
            writer.WriteUInt8((byte)(HasBeenInitialized ? 0x01 : 0x00));
        }

        public void Enqueue(DeltaSnapshotPack pack)
        {
            if (!IsValidPackToInsert(pack.tickIdRange))
            {
                throw new($"pack can not inserted {pack} {lastInsertedTickIdRange}");
            }


            HasBeenInitialized = true;

            var lastInsertedTickId = packs.Count > 0 ? lastInsertedTickIdRange.Last : (TickId?)null;

            packs.Enqueue(pack);
            lastInsertedTickIdRange = pack.tickIdRange;
            wantsTickIdValue = lastInsertedTickIdRange.Last.Next.tickId;
        }

        public DeltaSnapshotPack Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public int TicksAheadOfLastInQueue(TickId tickId)
        {
            if (packs.Count == 0)
            {
                return 0;
            }

            return (int)(LastInsertedTickIdRange.Last.tickId - tickId.tickId);
        }

        public bool IsValidPackToInsert(TickIdRange tickIdRange)
        {
            return !HasBeenInitialized || lastInsertedTickIdRange.CanAppend(tickIdRange);
        }

        public void Clear()
        {
            packs.Clear();
        }

        public DeltaSnapshotPack Peek()
        {
            return packs.Peek();
        }
    }
}