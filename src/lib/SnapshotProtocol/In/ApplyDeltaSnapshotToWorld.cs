/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class ApplyDeltaSnapshotToWorld
    {
        public static void Apply(DeltaSnapshotPack pack, IEntityContainerWithGhostCreator world,
            bool isOverlappingMergedSnapshot)
        {
            switch (pack.PackType)
            {
                case DeltaSnapshotPackType.OctetStream:
                {
                    var snapshotReader = new OctetReader(pack.payload.Span);
                    SnapshotDeltaReader.Read(snapshotReader, world);
                }
                    break;
                case DeltaSnapshotPackType.BitStream:
                {
                    // TODO: Serialize exact bit count
                    var bitSnapshotReader =
                        new BitReader(pack.payload.Span, pack.payload.Length * 8);

                    SnapshotDeltaBitReader.ReadAndApply(bitSnapshotReader, world, isOverlappingMergedSnapshot);
                }
                    break;
            }
        }
    }
}