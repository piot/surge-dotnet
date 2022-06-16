/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotDeltaReader
    {
        public static SerializedSnapshotDelta Read(IOctetReader reader)
        {
            var frameIdRange = SnapshotIdRangeReader.Read(reader);
            var payloadOctetCount = reader.ReadUInt16();
            var payload = reader.ReadOctets(payloadOctetCount);
            return new()
            {
                snapshotIdRange = frameIdRange,
                payload = payload
            };
        }
    }

    public static class SnapshotDeltaWriter
    {
        public static void Write(IOctetWriter writer, SerializedSnapshotDelta serializedSnapshotDelta)
        {
            SnapshotIdRangeWriter.Write(writer, serializedSnapshotDelta.snapshotIdRange);
            writer.WriteUInt16((ushort)serializedSnapshotDelta.payload.Length);
            writer.WriteOctets(serializedSnapshotDelta.payload);
        }
    }


    public struct SerializedSnapshotDelta
    {
        public SnapshotIdRange snapshotIdRange;
        public byte[] payload;
    }
}