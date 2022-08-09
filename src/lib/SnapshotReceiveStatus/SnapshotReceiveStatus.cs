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
        ///     Sent from client to host. Client describes the last delta compressed snapshot for TickId it has received
        ///     and how many snapshots it has detected as dropped after that.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="lastReceivedTickId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Write(IOctetWriter writer, TickId lastReceivedTickId, byte droppedFramesAfterThat)
        {
            TickIdWriter.Write(writer, lastReceivedTickId);
            writer.WriteUInt8(droppedFramesAfterThat);
        }
    }

    public static class SnapshotReceiveStatusReader
    {
        /// <summary>
        ///     Read on host coming from client.
        ///     Client describes the last delta compressed snapshot TickId it has received and how many snapshots it has detected as
        ///     dropped after that.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="lastReceivedTickId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Read(IOctetReader reader, out TickId lastReceivedTickId,
            out byte droppedFramesAfterThat)
        {
            lastReceivedTickId = TickIdReader.Read(reader);
            droppedFramesAfterThat = reader.ReadUInt8();
        }
    }
}