/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Corrections;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class DeltaSnapshotIncludingCorrectionsReader
    {
        /// <summary>
        ///     Read on the client coming from the host.
        ///     In many cases the union only consists of a single delta compressed snapshot.
        ///     But in the case of previously dropped snapshots, host resends snapshots in
        ///     ascending, consecutive order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SnapshotDeltaPackIncludingCorrections Read(IOctetReader reader)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.UnionSync)
            {
                throw new Exception("out of sync");
            }
#endif
            var frameIdRange = TickIdRangeReader.Read(reader);
            var payloadOctetCount = reader.ReadUInt16();
            var payload = reader.ReadOctets(payloadOctetCount);

            return new(frameIdRange, payload);
        }
    }
}