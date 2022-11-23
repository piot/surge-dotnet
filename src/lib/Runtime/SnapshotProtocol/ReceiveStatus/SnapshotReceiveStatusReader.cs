/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.SnapshotProtocol.ReceiveStatus
{
    public static class SnapshotReceiveStatusReader
    {
        /// <summary>
        ///     Read on host coming from client.
        ///     Client describes the last delta compressed snapshot TickId it has received and how many snapshots it has
        ///     detected as dropped after that.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expectingTickId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Read(IOctetReader reader, out TickId expectingTickId,
            out byte droppedFramesAfterThat)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotReceiveStatusSync)
            {
                throw new("desync");
            }
#endif
            expectingTickId = TickIdReader.Read(reader);
            droppedFramesAfterThat = reader.ReadUInt8();
        }
    }
}