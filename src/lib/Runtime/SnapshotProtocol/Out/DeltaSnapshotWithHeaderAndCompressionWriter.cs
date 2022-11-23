/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Compress;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Serialization;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public static class DeltaSnapshotWithHeaderAndCompressionWriter
    {
        public static void Write(DeltaSnapshotPack deltaSnapshotIncludingPredictionAssignmentPack,
            IMultiCompressor multiCompressor, CompressorIndex compressorIndex, IOctetWriter writer,
            IOctetWriterWithResult subWriter)
        {
#if DEBUG
            writer.WriteUInt8(Constants.DeltaSnapshotIncludingCorrectionsSync);
#endif
            var snapshotMode = (byte)0;

            snapshotMode |= (byte)((compressorIndex.Index & 0x03) << 2);

            snapshotMode |=
                (byte)(SnapshotTypeWriter.ToSnapshotMode(deltaSnapshotIncludingPredictionAssignmentPack.SnapshotType) << 4);

            writer.WriteUInt8(snapshotMode);

            subWriter.WriteUInt16((ushort)deltaSnapshotIncludingPredictionAssignmentPack.payload.Length);
            subWriter.WriteOctets(deltaSnapshotIncludingPredictionAssignmentPack.payload.Span);

            var subPack = multiCompressor.Compress(compressorIndex.Index, subWriter.Octets);
            writer.WriteOctets(subPack);
        }
    }
}