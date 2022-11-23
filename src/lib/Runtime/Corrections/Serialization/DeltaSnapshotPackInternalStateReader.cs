/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections.Serialization
{
    public static class DeltaSnapshotPackInternalStateReader
    {
        public static DeltaSnapshotPack Read(IOctetReader reader)
        {
            var range = TickIdRangeReader.Read(reader);
            var type = (SnapshotType)reader.ReadUInt8();
            var octetCount = reader.ReadUInt16();


            return new(range, reader.ReadOctets(octetCount), type);
        }
    }
}