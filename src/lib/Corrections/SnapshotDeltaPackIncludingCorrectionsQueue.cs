/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections
{
    public sealed class SnapshotDeltaPackIncludingCorrectionsQueue : ISnapshotDeltaPackQueue, IOctetSerializable
    {
        private readonly Queue<SnapshotDeltaPackIncludingCorrectionsItem> packs = new();

        private TickIdRange lastInsertedTickIdRange;

        private uint wantsTickIdValue = 1;

        public TickId WantsTickId => new(wantsTickIdValue);

        public bool LastInsertedIsMergedSnapshot => HasBeenInitialized && lastInsertedTickIdRange.Length > 1;
        public TickIdRange LastInsertedTickIdRange => lastInsertedTickIdRange;

        public bool HasBeenInitialized { get; private set; }

        public void Deserialize(IOctetReader reader)
        {
            var count = reader.ReadUInt8();
            var queue = new Queue<SnapshotDeltaPackIncludingCorrectionsItem>(count);
            for (var i = 0; i < count; ++i)
            {
                var newItem = new SnapshotDeltaPackIncludingCorrectionsItem(
                    new(lastInsertedTickIdRange, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty,
                        SnapshotStreamType.BitStream, SnapshotType.CompleteState), new(0));
                newItem.Deserialize(reader);
                queue.Enqueue(newItem);
            }
        }

        public void Serialize(IOctetWriter writer)
        {
            writer.WriteUInt8((byte)packs.Count);
            foreach (var pack in packs)
            {
                pack.Serialize(writer);
            }

            TickIdRangeWriter.Write(writer, lastInsertedTickIdRange);
            TickIdWriter.Write(writer, WantsTickId);
            writer.WriteUInt8((byte)(LastInsertedIsMergedSnapshot ? 0x01 : 0x00));
            writer.WriteUInt8((byte)(HasBeenInitialized ? 0x01 : 0x00));
        }

        public void Enqueue(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!IsValidPackToInsert(pack.tickIdRange))
            {
                throw new Exception($"pack can not inserted {pack} {lastInsertedTickIdRange}");
            }


            HasBeenInitialized = true;

            var lastInsertedTickId = packs.Count > 0 ? lastInsertedTickIdRange.Last : (TickId?)null;

            packs.Enqueue(new(pack, lastInsertedTickId));
            lastInsertedTickIdRange = pack.tickIdRange;
            wantsTickIdValue = lastInsertedTickIdRange.Last.Next.tickId;
        }

        public SnapshotDeltaPackIncludingCorrectionsItem Dequeue()
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

        public SnapshotDeltaPackIncludingCorrectionsItem Peek()
        {
            return packs.Peek();
        }
    }
}