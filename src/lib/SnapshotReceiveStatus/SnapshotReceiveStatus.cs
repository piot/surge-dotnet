/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public static class SnapshotReceiveStatusWriter
    {
        public static void Write(IOctetWriter writer, SnapshotId lastReceivedSnapshotId, byte droppedFramesAfterThat)
        {
            SnapshotIdWriter.Write(writer, lastReceivedSnapshotId);
            writer.WriteUInt8(droppedFramesAfterThat);
        }
    }

    public static class SnapshotReceiveStatusReader
    {
        public static void Read(IOctetReader reader, out SnapshotId lastReceivedSnapshotId,
            out byte droppedFramesAfterThat)
        {
            lastReceivedSnapshotId = SnapshotIdReader.Read(reader);
            droppedFramesAfterThat = reader.ReadUInt8();
        }
    }
}