/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class ApplyDeltaSnapshotToWorld
    {
        public static void Apply(DeltaSnapshotPack pack, IEntityContainerWithGhostCreator world,
            bool isOverlappingMergedSnapshot)
        {
            switch (pack.StreamType)
            {
                case SnapshotStreamType.OctetStream:
                {
                    var snapshotReader = new OctetReader(pack.payload.Span);
                    SnapshotDeltaReader.Read(snapshotReader, world);
                }
                    break;
                case SnapshotStreamType.BitStream:
                {
                    // TODO: Serialize exact bit count
                    var bitSnapshotReader =
                        new BitReader(pack.payload.Span, pack.payload.Length * 8);

                    if (pack.SnapshotType == SnapshotType.DeltaSnapshot)
                    {
                        SnapshotDeltaBitReader.ReadAndApply(bitSnapshotReader, world, isOverlappingMergedSnapshot);
                    }
                    else
                    {
                        CompleteStateBitReader.ReadAndApply(bitSnapshotReader, world);
                    }
                }
                    break;
            }
        }
    }
}