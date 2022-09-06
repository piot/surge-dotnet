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
using Piot.Surge.Replay.Serialization;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;

namespace Piot.Surge.Replay
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ReplayPlayback
    {
        private readonly ILog log;
        private readonly ReplayReader replayReader;
        private readonly TimeTicker timeTicker;
        private readonly IEntityContainerWithGhostCreator world;
        private DeltaState? nextDeltaState;
        private TickId playbackTickId;

        public ReplayPlayback(IEntityContainerWithGhostCreator world, Milliseconds now,
            SemanticVersion expectedApplicationVersion, IOctetReaderWithSeekAndSkip reader, ILog log)
        {
            replayReader = new(reader);
            if (!expectedApplicationVersion.IsEqualDisregardSuffix(replayReader.ApplicationVersion))
            {
                throw new Exception(
                    $"version mismatch, can not use this replay file {replayReader.ApplicationVersion} vs expected {expectedApplicationVersion}");
            }

            if (!replayReader.StateSerializationVersion.IsEqualDisregardSuffix(SurgeConstants
                    .SnapshotSerializationVersion))
            {
                throw new Exception(
                    $"replay file is unsupported by current surge version {SurgeConstants.SnapshotSerializationVersion} vs file {replayReader.StateSerializationVersion}");
            }

            this.log = log;
            this.world = world;
            timeTicker = new(now, PlaybackTick, new(100), log.SubLog("ReplayPlayback"));
            var completeState = replayReader.Seek(replayReader.FirstCompleteStateTickId);
            playbackTickId = replayReader.FirstCompleteStateTickId;
            ApplyCompleteState(completeState);
            nextDeltaState = replayReader.ReadDeltaState();
        }

        private void ApplyCompleteState(CompleteState completeState)
        {
            var bitReader = new BitReader(completeState.Payload, completeState.Payload.Length * 8);
            CompleteStateBitReader.ReadAndApply(bitReader, world);
        }

        private void ApplyDeltaState(DeltaState deltaState)
        {
            var bitReader = new BitReader(deltaState.Payload, deltaState.Payload.Length * 8);
            var isOverlapping = deltaState.TickIdRange.Contains(playbackTickId);
            SnapshotDeltaBitReader.ReadAndApply(bitReader, world, isOverlapping);
            playbackTickId = deltaState.TickIdRange.Last;
        }

        private void PlaybackTick()
        {
            log.Debug("===Playback Tick()=== {TickId}", playbackTickId);
            if (nextDeltaState is null)
            {
                log.Notice("end of playback stream");
                return;
            }

            if (nextDeltaState.TickIdRange.Last > playbackTickId)
            {
                playbackTickId = playbackTickId.Next();
                log.Notice("not time yet to read next tickId");
            }

            Ticker.Tick(world);
            Notifier.Notify(world.AllEntities);
            OverWriter.Overwrite(world);
            log.Debug("===Playback Tick()=== Applying {TickId}", playbackTickId);
            ApplyDeltaState(nextDeltaState);
            playbackTickId = playbackTickId.Next();
            nextDeltaState = replayReader.ReadDeltaState();
        }

        public void Update(Milliseconds now)
        {
            timeTicker.Update(now);
        }
    }
}