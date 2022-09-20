/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections.Serialization
{
    public static class SnapshotDeltaPackIncludingCorrectionsWriter
    {
        public static void Write(SnapshotDeltaPackIncludingCorrections pack, IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)pack.deltaSnapshotPackPayload.Length);
            writer.WriteOctets(pack.deltaSnapshotPackPayload.Span);

            writer.WriteUInt16((ushort)pack.physicsCorrections.Length);
            writer.WriteOctets(pack.physicsCorrections.Span);

            TickIdRangeWriter.Write(writer, pack.tickIdRange);
            writer.WriteUInt8((byte)pack.StreamType);
            writer.WriteUInt8((byte)pack.SnapshotType);
        }
    }
}