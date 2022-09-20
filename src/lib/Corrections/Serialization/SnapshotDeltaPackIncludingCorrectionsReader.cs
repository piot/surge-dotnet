/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections.Serialization
{
    public static class SnapshotDeltaPackIncludingCorrectionsReader
    {
        public static SnapshotDeltaPackIncludingCorrections Read(IOctetReader reader)
        {
            var packCount = reader.ReadUInt16();
            var deltaSnapshotPackPayload = reader.ReadOctets(packCount).ToArray();

            var physicsOctetCount = reader.ReadUInt16();
            var physicsCorrections = reader.ReadOctets(physicsOctetCount).ToArray();

            var tickIdRange = TickIdRangeReader.Read(reader);
            var streamType = (SnapshotStreamType)reader.ReadUInt8();
            var snapshotType = (SnapshotType)reader.ReadUInt8();

            return new(tickIdRange, deltaSnapshotPackPayload, physicsCorrections, streamType, snapshotType);
        }
    }
}