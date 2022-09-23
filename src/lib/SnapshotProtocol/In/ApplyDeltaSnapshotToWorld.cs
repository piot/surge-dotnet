/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Event;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class ApplyDeltaSnapshotToWorld
    {
        public static EventSequenceId Apply(DeltaSnapshotPack pack, IEntityContainerWithGhostCreator world,
            IEventProcessor eventProcessor, EventSequenceId expectedEventSequenceId,
            bool isOverlappingMergedSnapshot, bool notifyWorldSync)
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
                        expectedEventSequenceId = SnapshotDeltaBitReader.ReadAndApply(bitSnapshotReader, world,
                            eventProcessor,
                            expectedEventSequenceId, isOverlappingMergedSnapshot, notifyWorldSync);
                    }
                    else
                    {
                        expectedEventSequenceId =
                            CompleteStateBitReader.ReadAndApply(bitSnapshotReader, world, eventProcessor,
                                notifyWorldSync);
                    }
                }
                    break;
            }

            return expectedEventSequenceId;
        }
    }
}