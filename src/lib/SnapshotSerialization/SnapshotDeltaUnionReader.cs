/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    
    public static class Constants
    {
        public const byte UnionSync = 0xba;
    }

    public static class SnapshotDeltaUnionReader
    {
        /// <summary>
        /// Read on the client coming from the host.
        /// In many cases the union only consists of a single delta compressed snapshot.
        /// But in the case of previously dropped snapshots, host resends snapshots in
        /// ascending, consecutive order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SerializedSnapshotDeltaPackUnion Read(IOctetReader reader)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.UnionSync)
            {
                throw new Exception("out of sync");
            }
#endif
            var frameIdRange = SnapshotIdRangeReader.Read(reader);
            var packs = new SnapshotDeltaPack.SnapshotDeltaPack[frameIdRange.Length];
            for (var i = 0; i < frameIdRange.Length; ++i)
            {
                var payloadOctetCount = reader.ReadUInt16();
                var payload = reader.ReadOctets(payloadOctetCount);
                var snapshotDeltaPack = new SnapshotDeltaPack.SnapshotDeltaPack(new SnapshotId((uint)(frameIdRange.containsFromSnapshotId.frameId + i)), payload);
                packs[i] = snapshotDeltaPack;
            }

            return new SerializedSnapshotDeltaPackUnion
            {
                snapshotIdRange = frameIdRange,
                packs = packs,
            };
        }
    }


    public static class SnapshotDeltaUnionWriter
    {
        /// <summary>
        /// Written on the host to be sent to a client.
        /// In many cases the union only consists of a single delta compressed snapshot.
        /// But in the case of previously dropped snapshots, host resends snapshots in
        /// ascending, consecutive order.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="serializedSnapshotDeltaPacks"></param>
        public static void Write(IOctetWriter writer, SerializedSnapshotDeltaPackUnion serializedSnapshotDeltaPacks)
        {
            #if DEBUG
                writer.WriteUInt8(Constants.UnionSync);
            #endif
            SnapshotIdRangeWriter.Write(writer, serializedSnapshotDeltaPacks.snapshotIdRange);
            foreach (var serializedSnapshotDeltaPack in serializedSnapshotDeltaPacks.packs)
            {
                writer.WriteUInt16((ushort)serializedSnapshotDeltaPack.payload.Length);
                writer.WriteOctets(serializedSnapshotDeltaPack.payload);
            }
        }
    }


    public static class SnapshotDeltaUnionPacker
    {
        public static SerializedSnapshotDeltaPackUnionFlattened Pack(SerializedSnapshotDeltaPackUnion serializedSnapshotDeltaPacks)
        {
            var unionStream = new OctetWriter(10 * 1200);
            
            SnapshotDeltaUnionWriter.Write(unionStream, serializedSnapshotDeltaPacks);
            return new SerializedSnapshotDeltaPackUnionFlattened
            {
                snapshotIdRange = serializedSnapshotDeltaPacks.snapshotIdRange,
                payload = unionStream.Octets
            };
        }

    }

    public struct SerializedSnapshotDeltaPackUnion
    {
        public SnapshotIdRange snapshotIdRange;
        public SnapshotDeltaPack.SnapshotDeltaPack[] packs;
       
    }
    
    public struct SerializedSnapshotDeltaPackUnionFlattened
    {
        public SnapshotIdRange snapshotIdRange;
        public Memory<byte> payload;
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