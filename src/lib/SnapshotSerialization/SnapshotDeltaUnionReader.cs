/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotSerialization
{
    public static class Constants
    {
        public const byte UnionSync = 0xba;
        public const byte SnapshotDeltaSync = 0xbb;
        public const byte SnapshotDeltaCreatedSync = 0xbc;
        public const byte SnapshotDeltaUpdatedSync = 0x13;
    }

    public static class SnapshotDeltaUnionReader
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

            return new (frameIdRange, payload);
        }
    }
}