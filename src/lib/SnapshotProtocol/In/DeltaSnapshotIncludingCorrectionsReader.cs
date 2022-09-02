/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Compress;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class DeltaSnapshotIncludingCorrectionsReader
    {
        /// <summary>
        ///     ReadAndApply on the client coming from the host.
        ///     In many cases the union only consists of a single delta compressed snapshot.
        ///     But in the case of previously dropped snapshots, host resends snapshots in
        ///     ascending, consecutive order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SnapshotDeltaPackIncludingCorrections Read(TickIdRange tickIdRange,
            ReadOnlySpan<byte> datagramOctets, IMultiCompressor multiCompressor)
        {
            var headerSize = 1;
#if DEBUG
            headerSize++;
#endif
            var headerReader = new OctetReader(datagramOctets.Slice(0, headerSize));
#if DEBUG
            if (headerReader.ReadUInt8() != Constants.DeltaSnapshotIncludingCorrectionsSync)
            {
                throw new Exception("out of sync");
            }
#endif
            var snapshotMode = headerReader.ReadUInt8();
            var dataType = (snapshotMode & 0x03) switch
            {
                0x00 => DeltaSnapshotPackType.BitStream,
                0x01 => DeltaSnapshotPackType.OctetStream
            };

            var compressionIndex = (uint)(snapshotMode >> 2);
            var rest = datagramOctets[headerSize..];

            var reader = new OctetReader(multiCompressor.Decompress(compressionIndex, rest));

            var payloadOctetCount = reader.ReadUInt16();
            var payload = reader.ReadOctets(payloadOctetCount);

            var physicsCorrectionOctetCount = reader.ReadUInt16();
            var physicsCorrection = reader.ReadOctets(physicsCorrectionOctetCount);

            return new(tickIdRange, payload, physicsCorrection, dataType);
        }
    }
}