/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge.SnapshotReceiveStatus
{
    public static class Constants
    {
        public const byte SnapshotReceiveStatusSync = 0x18;
    }

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
#if DEBUG
            writer.WriteUInt8(Constants.SnapshotReceiveStatusSync);
#endif

            TickIdWriter.Write(writer, lastReceivedTickId);
            writer.WriteUInt8(droppedFramesAfterThat);
        }
    }

    public static class SnapshotReceiveStatusReader
    {
        /// <summary>
        ///     Read on host coming from client.
        ///     Client describes the last delta compressed snapshot TickId it has received and how many snapshots it has
        ///     detected as dropped after that.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="lastReceivedTickId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Read(IOctetReader reader, out TickId lastReceivedTickId,
            out byte droppedFramesAfterThat)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotReceiveStatusSync)
            {
                throw new Exception("desync");
            }
#endif
            lastReceivedTickId = TickIdReader.Read(reader);
            droppedFramesAfterThat = reader.ReadUInt8();
        }
    }
}