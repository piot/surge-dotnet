/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotDeltaUnionReader
    {
        public static SerializedSnapshotDeltaPackUnion Read(IOctetReader reader)
        {
            var frameIdRange = SnapshotIdRangeReader.Read(reader);
            var packs = new SerializedSnapshotDeltaPack[frameIdRange.Length];
            for (var i = 0; i < frameIdRange.Length; ++i)
            {
                var payloadOctetCount = reader.ReadUInt16();
                var payload = reader.ReadOctets(payloadOctetCount);
                var snapshotDeltaPack = new SerializedSnapshotDeltaPack()
                {
                    snapshotId = new SnapshotId((uint)(frameIdRange.containsFromSnapshotId.frameId + i)),
                    payload = payload
                };
                packs[i] = snapshotDeltaPack;
            }

            return new SerializedSnapshotDeltaPackUnion
            {
                snapshotIdRange = frameIdRange,
                packs = packs,
            };
        }
    }

    public static class SnapshotDeltaWriter
    {
        public static void Write(IOctetWriter writer, SerializedSnapshotDeltaPackUnion serializedSnapshotDeltaPacks)
        {
            SnapshotIdRangeWriter.Write(writer, serializedSnapshotDeltaPacks.snapshotIdRange);
            foreach (var serializedSnapshotDeltaPack in serializedSnapshotDeltaPacks.packs)
            {
                writer.WriteUInt16((ushort)serializedSnapshotDeltaPack.payload.Length);
                writer.WriteOctets(serializedSnapshotDeltaPack.payload);
            }
        }
    }


    public struct SerializedSnapshotDeltaPackUnion
    {
        public SnapshotIdRange snapshotIdRange;
        public SerializedSnapshotDeltaPack[] packs;
    }
    
    public struct SerializedSnapshotDeltaPack
    {
        public SnapshotId snapshotId;
        public byte[] payload;
    }
    
    public struct SerializedSnapshotDeltaPackForEntity
    {
        public SnapshotId snapshotId;
        public EntityId entityId;
        public byte[] payload;
    }
}