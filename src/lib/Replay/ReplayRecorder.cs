/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.Tick;

namespace Piot.Surge.Replay
{
    public class ReplayRecorder
    {
        private readonly ILog log;
        private readonly ReplayWriter replayWriter;
        private readonly IEntityContainer world;

        public ReplayRecorder(IEntityContainer world, TickId nowTickId,
            IOctetWriter writer, ILog log)
        {
            this.log = log.SubLog("ReplayRecorder");
            this.world = world;
            var completeState = CaptureCompleteState(nowTickId);
            replayWriter = new(completeState, writer);
        }

        private CompleteState CaptureCompleteState(TickId tickId)
        {
            var payload = CompleteStateBitWriter.CaptureCompleteSnapshotPack(world);
            var completeState = new CompleteState(tickId, payload);

            return completeState;
        }

        public void AddPack(DeltaSnapshotPack pack, TickId worldTickIdNow)
        {
            if (pack.TickIdRange.Last <= worldTickIdNow)
            {
                throw new Exception($"wrong order for complete state {pack.TickIdRange} {worldTickIdNow}");
            }

            if (replayWriter.NeedsCompleteState)
            {
                log.Info("Time to capture complete state {TickIdNow}", worldTickIdNow);
                replayWriter.AddCompleteState(CaptureCompleteState(worldTickIdNow));
            }

            log.Info("Add delta state {TickIdRange}", pack.tickIdRange);
            var deltaState = new DeltaState(pack.tickIdRange, pack.payload.Span);
            replayWriter.AddDeltaState(deltaState);
        }

        public void Close()
        {
            replayWriter.Close();
        }
    }
}