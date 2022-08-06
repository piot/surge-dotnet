/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotIdRangeReader
    {
        /**
         * Reads a snapshot ID range. The range is always with ascending, consecutive IDs.
         */
        public static SnapshotIdRange Read(IOctetReader reader)
        {
            var currentId = SnapshotIdReader.Read(reader);
            return new SnapshotIdRange
            {
                snapshotId = currentId,
                containsFromSnapshotId = new SnapshotId { frameId = currentId.frameId - reader.ReadUInt8() }
            };
        }
    }
}