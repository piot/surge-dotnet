/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public static class SnapshotPackDatagramHeaderReader
    {
        public static void Read(IOctetReader reader, out TickIdRange tickIdRange, out int datagramIndex,
            out bool isLastOne)
        {
            tickIdRange = TickIdRangeReader.Read(reader);

            var index = reader.ReadUInt8();
            isLastOne = (index & 0x80) != 0;
            datagramIndex = index & 0x7f;
        }
    }
}