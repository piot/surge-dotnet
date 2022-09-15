/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.Tick;

namespace Piot.Surge.Replay
{
    public sealed class ReplayRecorder
    {
        private readonly ILog log;
        private readonly ReplayWriter replayWriter;
        private readonly IEntityContainer world;

        public ReplayRecorder(IEntityContainer world, TimeMs timeNowMs, TickId nowTickId,
            SemanticVersion applicationVersion, IOctetWriter writer, ILog log)
        {
            this.log = log.SubLog("ReplayRecorder");
            this.world = world;
            var completeState = CaptureCompleteState(timeNowMs, nowTickId);
            var versionInfo = new ReplayVersionInfo(applicationVersion, SurgeConstants.SnapshotSerializationVersion);
            replayWriter = new(completeState, versionInfo, writer);
        }

        private CompleteState CaptureCompleteState(TimeMs timeNow, TickId tickId)
        {
            var payload = CompleteStateBitWriter.CaptureCompleteSnapshotPack(world, new(0));
            var completeState = new CompleteState(timeNow, tickId, payload);

            return completeState;
        }

        public void AddPack(DeltaSnapshotPack pack, TimeMs timeNowMs, TickId worldTickIdNow)
        {
            if (pack.TickIdRange.Last != worldTickIdNow)
            {
                throw new Exception($"wrong order for complete state {pack.TickIdRange} {worldTickIdNow}");
            }

            log.Info("Add delta state {TickIdRange}", pack.tickIdRange);
            var deltaState = new DeltaState(timeNowMs, pack.tickIdRange, pack.payload.Span);
            replayWriter.AddDeltaState(deltaState);

            if (!replayWriter.NeedsCompleteState)
            {
                return;
            }

            log.Info("Time to capture complete state {TickIdNow}", worldTickIdNow);
            replayWriter.AddCompleteState(CaptureCompleteState(timeNowMs, worldTickIdNow));
        }

        public void Close()
        {
            replayWriter.Close();
        }
    }
}