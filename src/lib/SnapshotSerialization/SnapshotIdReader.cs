/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotSerialization
{
    public static class SnapshotIdReader
    {
        public static SnapshotId Read(IOctetReader reader)
        {
            return new SnapshotId(reader.ReadUInt32());
        }
    }
}