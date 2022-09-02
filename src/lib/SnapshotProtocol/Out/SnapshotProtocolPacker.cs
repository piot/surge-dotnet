/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Compress;
using Piot.Surge.Corrections;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public static class SnapshotProtocolPacker
    {
        public static SnapshotProtocolPack Pack(SnapshotDeltaPackIncludingCorrections deltaPack,
            IMultiCompressor multiCompressor, CompressorIndex compressorIndex)
        {
            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);
            SnapshotIncludingCorrectionsWriter.Write(deltaPack, multiCompressor, compressorIndex, writer);
            return new(deltaPack.tickIdRange, writer.Octets);
        }
    }
}