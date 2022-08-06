/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotIdReader
    {
        /**
         * Reads a snapshot ID from the stream reader.
         */
        public static SnapshotId Read(IOctetReader reader)
        {
            return new SnapshotId(reader.ReadUInt32());
        }
    }
}