/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public static class SnapshotReceiveStatusWriter
    {
        /// <summary>
        /// Sent from client to host. Client describes the last delta compressed snapshot ID it has received
        /// and how many snapshots it has detected as dropped after that.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="lastReceivedSnapshotId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Write(IOctetWriter writer, SnapshotId lastReceivedSnapshotId, byte droppedFramesAfterThat)
        {
            SnapshotIdWriter.Write(writer, lastReceivedSnapshotId);
            writer.WriteUInt8(droppedFramesAfterThat);
        }
    }

    public static class SnapshotReceiveStatusReader
    {
        /// <summary>
        /// Read on host coming from client.
        /// Client describes the last delta compressed snapshot ID it has received and how many snapshots it has detected as dropped after that.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="lastReceivedSnapshotId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Read(IOctetReader reader, out SnapshotId lastReceivedSnapshotId,
            out byte droppedFramesAfterThat)
        {
            lastReceivedSnapshotId = SnapshotIdReader.Read(reader);
            droppedFramesAfterThat = reader.ReadUInt8();
        }
    }
}