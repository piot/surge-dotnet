/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotIdWriter
    {
        /// <summary>
        ///     Writes a snapshot ID to the stream writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="snapshotId"></param>
        public static void Write(IOctetWriter writer, SnapshotId snapshotId)
        {
            writer.WriteUInt32(snapshotId.frameId);
        }
    }
}