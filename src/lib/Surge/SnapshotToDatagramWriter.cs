/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public class SnapshotPackToDatagramWriter
    {
        public void Write(Action<Memory<byte>> send, SerializedSnapshotDeltaPackUnionFlattened pack,
            OrderedDatagramsOut orderedDatagramsHeader)
        {
            var writer = new OctetWriter(1200);

            var datagramCount = pack.payload.Length / 1100 + 1;

            for (var datagramIndex = 0; datagramIndex < datagramCount; ++datagramIndex)
            {
                orderedDatagramsHeader.Write(writer);
                SnapshotIdRangeWriter.Write(writer, pack.snapshotIdRange);
                var indexMask = (byte)datagramIndex;
                if (datagramIndex + 1 == datagramCount) indexMask |= 0x80;
                writer.WriteUInt8(indexMask);

                writer.WriteOctets(pack.payload);

                send(writer.Octets);
            }
        }
    }
}