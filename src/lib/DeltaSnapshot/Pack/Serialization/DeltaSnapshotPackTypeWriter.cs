/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.DeltaSnapshot.Pack.Serialization
{
    public static class DeltaSnapshotPackTypeConverter
    {
        public static byte ToSnapshotMode(DeltaSnapshotPackType packType)
        {
            return packType switch
            {
                DeltaSnapshotPackType.BitStream => 0x00,
                DeltaSnapshotPackType.OctetStream => 0x01
            };
        }
    }
}