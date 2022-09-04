/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;

namespace Piot.Surge.Replay
{
    public class ReplayPlayback
    {
        private readonly ILog log;
        private readonly ReplayReader replayReader;
        private readonly TimeTicker timeTicker;
        private readonly IEntityContainerWithGhostCreator world;
        private DeltaState? nextDeltaState;
        private TickId playbackTickId;

        public ReplayPlayback(IEntityContainerWithGhostCreator world, Milliseconds now,
            IOctetReaderWithSeekAndSkip reader, ILog log)
        {
            replayReader = new(reader);
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

            log.Debug("===Playback Tick()=== Applying {TickId}", playbackTickId);
            ApplyDeltaState(nextDeltaState);
            Ticker.Tick(world);
            Notifier.Notify(world.AllEntities);
            playbackTickId = playbackTickId.Next();
            nextDeltaState = replayReader.ReadDeltaState();
        }

        public void Update(Milliseconds now)
        {
            timeTicker.Update(now);
        }
    }
}