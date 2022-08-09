/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotIdRangeWriter
    {
        /// <summary>
        ///     Writes a snapshot ID range to the stream.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="snapshotIdRange"></param>
        public static void Write(IOctetWriter writer, SnapshotIdRange snapshotIdRange)
        {
            SnapshotIdWriter.Write(writer, snapshotIdRange.snapshotId);
            writer.WriteUInt8(
                (byte)(snapshotIdRange.snapshotId.frameId - snapshotIdRange.containsFromSnapshotId.frameId));
        }
    }
}