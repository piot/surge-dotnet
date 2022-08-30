/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.SnapshotProtocol.Fragment
{
    public static class SnapshotFragmentHeaderReader
    {
        public static void Read(IOctetReader reader, out TickIdRange tickIdRange, out int datagramIndex, out ushort octetCount,
            out bool isLastOne)
        {
            tickIdRange = TickIdRangeReader.Read(reader);

            var index = reader.ReadUInt8();
            isLastOne = (index & 0x80) != 0;
            datagramIndex = index & 0x7f;

            octetCount = reader.ReadUInt16();
        }
    }
}