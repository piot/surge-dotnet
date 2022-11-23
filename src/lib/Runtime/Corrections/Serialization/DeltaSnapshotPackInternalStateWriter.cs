/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections.Serialization
{
    public static class DeltaSnapshotPackInternalStateWriter
    {
        public static void Write(DeltaSnapshotPack item, IOctetWriter writer)
        {
            TickIdRangeWriter.Write(writer, item.tickIdRange);
            writer.WriteUInt16((ushort)item.payload.Length);
            writer.WriteOctets(item.payload.Span);
        }
    }
}