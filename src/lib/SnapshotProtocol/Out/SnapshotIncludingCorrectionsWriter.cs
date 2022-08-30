/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Corrections;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public static class SnapshotIncludingCorrectionsWriter
    {
        public static void Write(SnapshotDeltaPackIncludingCorrections deltaSnapshotIncludingCorrectionsPack,
            IOctetWriter writer)
        {
#if DEBUG
            writer.WriteUInt8(Constants.UnionSync);
#endif
            writer.WriteUInt16((ushort)deltaSnapshotIncludingCorrectionsPack.payload.Length);
            writer.WriteOctets(deltaSnapshotIncludingCorrectionsPack.payload.Span);
        }
    }
}