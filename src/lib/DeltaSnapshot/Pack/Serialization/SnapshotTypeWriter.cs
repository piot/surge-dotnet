/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.DeltaSnapshot.Pack.Serialization
{
    public static class SnapshotTypeWriter
    {
        public static byte ToSnapshotMode(SnapshotType snapshotType)
        {
            return snapshotType switch
            {
                SnapshotType.CompleteState => 0x00,
                SnapshotType.DeltaSnapshot => 0x01
            };
        }
    }
}