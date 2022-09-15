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
using Piot.Surge.Event;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;

namespace Piot.Surge.Replay
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ReplayPlayback
    {
        private readonly IEventProcessor eventProcessor;
        private readonly ILog log;
        private readonly ReplayReader replayReader;
        private readonly TimeTicker timeTicker;
        private readonly IEntityContainerWithGhostCreator world;
        private TickId lastAppliedTickId;
        private DeltaState? nextDeltaState;
        private EventSequenceId nextExpectedSequenceId;
        private TickId playbackTickId;

        public ReplayPlayback(IEntityContainerWithGhostCreator world,
            IEventProcessor eventProcessor, TimeMs now,
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
            this.eventProcessor = eventProcessor;
            timeTicker = new(now, PlaybackTick, new(20), log.SubLog("ReplayPlayback"));
            var completeState = replayReader.Seek(replayReader.FirstCompleteStateTickId);
            playbackTickId = replayReader.FirstCompleteStateTickId;
            log.Info("Playback starts at {TickId}", playbackTickId);
            ApplyCompleteState(completeState);
            nextDeltaState = replayReader.ReadDeltaState();
        }

        private void ApplyCompleteState(CompleteState completeState)
        {
            log.DebugLowLevel("applying complete state {CompleteState}", completeState);
            var bitReader = new BitReader(completeState.Payload, completeState.Payload.Length * 8);
            nextExpectedSequenceId = CompleteStateBitReader.ReadAndApply(bitReader, world, eventProcessor);
            lastAppliedTickId = completeState.TickId;
        }

        private void ApplyDeltaState(DeltaState deltaState)
        {
            log.DebugLowLevel("applying delta state {DeltaState}", deltaState);
            var bitReader = new BitReader(deltaState.Payload, deltaState.Payload.Length * 8);
            if (!deltaState.TickIdRange.CanBeFollowing(lastAppliedTickId))
            {
                throw new Exception(
                    $"can not process this delta state, they are not in sequence {deltaState.TickIdRange} {lastAppliedTickId}");
            }

            var isOverlappingAndMerged = deltaState.TickIdRange.IsOverlappingAndMerged(lastAppliedTickId);
            nextExpectedSequenceId = SnapshotDeltaBitReader.ReadAndApply(bitReader, world, eventProcessor,
                nextExpectedSequenceId, isOverlappingAndMerged);
            playbackTickId = deltaState.TickIdRange.Last;
            lastAppliedTickId = deltaState.TickIdRange.Last;
        }

        private void PlaybackTick()
        {
            log.DebugLowLevel("===Playback Tick()=== {TickId}", playbackTickId);
            if (nextDeltaState is null)
            {
                log.Debug("end of playback stream");
                timeTicker.DeltaTime = new(0);
                return;
            }

            if (nextDeltaState.TickIdRange.Last > playbackTickId)
            {
                log.Debug(
                    "not time yet to read next tickId, we want to apply {TickId} but next state is at {NextStateTickIdRange}",
                    playbackTickId, nextDeltaState.TickIdRange);
                playbackTickId = playbackTickId.Next();
                return;
            }

            Ticker.Tick(world);
            Notifier.Notify(world.AllEntities);
            log.DebugLowLevel("===Playback Tick()=== Applying {TickId}", playbackTickId);
            ApplyDeltaState(nextDeltaState);
            playbackTickId = playbackTickId.Next();
            nextDeltaState = replayReader.ReadDeltaState();
        }

        public void Update(TimeMs now)
        {
            timeTicker.Update(now);
        }
    }
}