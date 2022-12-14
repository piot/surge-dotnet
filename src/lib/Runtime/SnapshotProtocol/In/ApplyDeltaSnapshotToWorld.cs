/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.Core;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Event;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class ApplyDeltaSnapshotToWorld
    {
        public static EventSequenceId Apply(IBitReader bitSnapshotReader, SnapshotType snapshotType, IDataReceiver world,
            IEventReceiver eventProcessor, EventSequenceId expectedEventSequenceId,
            bool isOverlappingMergedSnapshot, ILog log)
        {
            if (snapshotType == SnapshotType.DeltaSnapshot)
            {
                expectedEventSequenceId = SnapshotDeltaBitReader.ReadAndApply(bitSnapshotReader, world,
                    eventProcessor,
                    expectedEventSequenceId, isOverlappingMergedSnapshot, log);
            }
            else
            {
                expectedEventSequenceId =
                    CompleteStateBitReader.ReadAndApply(bitSnapshotReader, world, eventProcessor,
                        log);
            }

            return expectedEventSequenceId;
        }
    }
}