/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Compress;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Pack.Serialization;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public static class SnapshotIncludingCorrectionsWriter
    {
        public static void Write(SnapshotDeltaPackIncludingCorrections deltaSnapshotIncludingCorrectionsPack,
            IMultiCompressor multiCompressor, CompressorIndex compressorIndex, IOctetWriter writer,
            IOctetWriterWithResult subWriter)
        {
#if DEBUG
            writer.WriteUInt8(Constants.DeltaSnapshotIncludingCorrectionsSync);
#endif
            var snapshotMode =
                DeltaSnapshotPackTypeConverter.ToSnapshotMode(deltaSnapshotIncludingCorrectionsPack.StreamType);

            snapshotMode |= (byte)((compressorIndex.Index & 0x03) << 2);

            snapshotMode |=
                (byte)(SnapshotTypeWriter.ToSnapshotMode(deltaSnapshotIncludingCorrectionsPack.SnapshotType) << 4);

            writer.WriteUInt8(snapshotMode);

            subWriter.WriteUInt16((ushort)deltaSnapshotIncludingCorrectionsPack.deltaSnapshotPackPayload.Length);
            subWriter.WriteOctets(deltaSnapshotIncludingCorrectionsPack.deltaSnapshotPackPayload.Span);

            subWriter.WriteUInt16((ushort)deltaSnapshotIncludingCorrectionsPack.physicsCorrections.Length);
            subWriter.WriteOctets(deltaSnapshotIncludingCorrectionsPack.physicsCorrections.Span);

            var subPack = multiCompressor.Compress(compressorIndex.Index, subWriter.Octets);
            writer.WriteOctets(subPack);
        }
    }
}