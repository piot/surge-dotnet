/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Pack.Serialization;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public static class SnapshotIncludingCorrectionsWriter
    {
        public static void Write(SnapshotDeltaPackIncludingCorrections deltaSnapshotIncludingCorrectionsPack,
            IOctetWriter writer)
        {
#if DEBUG
            writer.WriteUInt8(Constants.DeltaSnapshotIncludingCorrectionsSync);
#endif
            var snapshotMode =
                DeltaSnapshotPackTypeConverter.ToSnapshotMode(deltaSnapshotIncludingCorrectionsPack.PackType);
            writer.WriteUInt8(snapshotMode);

            writer.WriteUInt16((ushort)deltaSnapshotIncludingCorrectionsPack.deltaSnapshotPackPayload.Length);
            writer.WriteOctets(deltaSnapshotIncludingCorrectionsPack.deltaSnapshotPackPayload.Span);

            writer.WriteUInt16((ushort)deltaSnapshotIncludingCorrectionsPack.physicsCorrections.Length);
            writer.WriteOctets(deltaSnapshotIncludingCorrectionsPack.physicsCorrections.Span);
        }
    }
}