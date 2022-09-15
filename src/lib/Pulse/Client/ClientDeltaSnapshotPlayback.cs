/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Stats;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Event;
using Piot.Surge.SnapshotProtocol.In;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;

namespace Piot.Surge.Pulse.Client
{
    /// <summary>
    ///     Enqueues delta snapshots and plays them back at varying delta time depending on number of delta snapshots in
    ///     snapshotsQueue.
    /// </summary>
    public sealed class ClientDeltaSnapshotPlayback
    {
        private readonly IEntityContainerWithGhostCreator clientWorld;
        private readonly IEventProcessor eventProcessor;

        private readonly HoldPositive lastBufferWasStarved = new(14);
        private readonly ILog log;
        private readonly IClientPredictorCorrections predictor;
        private readonly SnapshotPlaybackNotify snapshotPlaybackNotify;
        private readonly TimeTicker snapshotPlaybackTicker;
        private readonly SnapshotDeltaPackIncludingCorrectionsQueue snapshotsQueue = new();
        private readonly FixedDeltaTimeMs targetDeltaTimeMs;
        private EventSequenceId expectedEventSequenceId;
        private TickId playbackTick = new(0);

        public ClientDeltaSnapshotPlayback(TimeMs now, IEntityContainerWithGhostCreator clientWorld,
            IEventProcessor eventProcessor, IClientPredictorCorrections predictor,
            SnapshotPlaybackNotify snapshotPlaybackNotify,
            FixedDeltaTimeMs targetDeltaTimeMs, ILog log)
        {
            this.log = log;
            this.predictor = predictor;
            this.clientWorld = clientWorld;
            this.eventProcessor = eventProcessor;
            this.snapshotPlaybackNotify = snapshotPlaybackNotify;
            this.targetDeltaTimeMs = targetDeltaTimeMs;
            snapshotPlaybackTicker = new(now, NextSnapshotTick, targetDeltaTimeMs,
                log.SubLog("NextSnapshotTick"));
        }

        public bool LastPlaybackSnapshotWasSkipAhead { get; private set; }
        public bool LastPlaybackSnapshotWasMerged { get; private set; }
        public bool LastBufferWasStarved => lastBufferWasStarved.Value;

        public bool ShouldApplySnapshotsToWorld { get; set; } = true;
        public bool IsIncomingBufferStarving => lastBufferWasStarved.IsOrWasTrue;

        public void Update(TimeMs now)
        {
            snapshotPlaybackTicker.Update(now);
        }

        public bool WantsSnapshotWithTickIdRange(TickIdRange tickIdRange)
        {
            return snapshotsQueue.IsValidPackToInsert(tickIdRange);
        }

        public void FeedSnapshotDeltaPack(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!pack.tickIdRange.Contains(snapshotsQueue.WantsTickId))
            {
                log.Notice("unexpected incoming delta pack encountered {TickIdRange}, expected {TickId}",
                    pack.tickIdRange, snapshotsQueue.WantsTickId);
                return;
            }

            if (pack.tickIdRange.Last.tickId < snapshotsQueue.WantsTickId.tickId)
            {
                log.Notice("old incoming delta pack encountered {TickIdRange}, expected {TickId}", pack.tickIdRange,
                    snapshotsQueue.WantsTickId);
                return;
            }

            if (!snapshotsQueue.IsValidPackToInsert(pack.tickIdRange))
            {
                throw new DeserializeException("wrong pack id ordering");
            }

            snapshotsQueue.Enqueue(pack);

            if (snapshotsQueue.LastInsertedIsMergedSnapshot)
            {
                log.Notice("Receiving merged snapshots {Range}", snapshotsQueue.LastInsertedTickIdRange);
            }
        }

        private void NextSnapshotTick()
        {
            var targetDeltaTimeMsValue = targetDeltaTimeMs.ms;
            // Our goal is to have just two snapshots in the snapshotsQueue.
            // So adjust the playback speed using the playback delta time.
            var bufferAheadCount = snapshotsQueue.TicksAheadOfLastInQueue(playbackTick);
            var deltaTimeMs = bufferAheadCount switch
            {
                < 2 => targetDeltaTimeMsValue * 110 / 100,
                > 4 => targetDeltaTimeMsValue * 70 / 100,
                _ => targetDeltaTimeMsValue
            };

            log.DebugLowLevel(
                "Try to read next snapshot in snapshotsQueue. {BufferAheadCount} {PlaybackTickId} {PlaybackDeltaTimeMs}",
                bufferAheadCount, playbackTick, deltaTimeMs);

            snapshotPlaybackTicker.DeltaTime = new(deltaTimeMs);

            if (!snapshotsQueue.HasBeenInitialized)
            {
                log.DebugLowLevel("Not trying to playback yet, waiting for first incoming snapshot");
                return;
            }


            lastBufferWasStarved.Value = bufferAheadCount < 2;

            if (snapshotsQueue.Count == 0)
            {
                log.Notice(
                    "Snapshot playback has stalled because incoming snapshot snapshotsQueue is empty");
                return;
            }

            playbackTick = new(playbackTick.tickId + 1);

            if (snapshotsQueue.Peek().Pack.tickIdRange.Last > playbackTick)
            {
                log.Notice(
                    "Snapshot playback has stalled because next snapshot is in the future compared to playback");
                return;
            }

            var deltaSnapshotIncludingCorrectionsItem = snapshotsQueue.Dequeue();


            var deltaSnapshotIncludingCorrections = deltaSnapshotIncludingCorrectionsItem.Pack;
            log.DebugLowLevel("dequeued snapshot {DeltaSnapshotEntityIds}", deltaSnapshotIncludingCorrections);

            var deltaSnapshotPack = new DeltaSnapshotPack(deltaSnapshotIncludingCorrections.tickIdRange,
                deltaSnapshotIncludingCorrections.deltaSnapshotPackPayload.Span,
                deltaSnapshotIncludingCorrections.StreamType, deltaSnapshotIncludingCorrections.SnapshotType);

            snapshotPlaybackNotify.Invoke(snapshotPlaybackTicker.Now, playbackTick, deltaSnapshotPack);

            LastPlaybackSnapshotWasSkipAhead = deltaSnapshotIncludingCorrectionsItem.IsSkippedAheadSnapshot;
            LastPlaybackSnapshotWasMerged = deltaSnapshotIncludingCorrectionsItem.IsMergedAndOverlapping;

            if (!ShouldApplySnapshotsToWorld)
            {
                snapshotsQueue.Clear();
                return;
            }

            expectedEventSequenceId = ApplyDeltaSnapshotToWorld.Apply(deltaSnapshotPack, clientWorld,
                eventProcessor, expectedEventSequenceId,
                deltaSnapshotIncludingCorrectionsItem.IsMergedAndOverlapping);

            predictor.AssignAvatarAndReadCorrections(deltaSnapshotIncludingCorrections.tickIdRange.Last,
                deltaSnapshotIncludingCorrections.physicsCorrections.Span);

            // Ghosts are never predicted, corrected, nor rolled back
            // All the changed fields are set to the new values and Tick() is called to trigger the resulting effects of
            // the logic running for one tick.
            ChangeClearer.ClearChanges(clientWorld);
            Ticker.Tick(clientWorld);
            Notifier.Notify(clientWorld.AllEntities);

            log.DebugLowLevel("tick ghost logics {EntityCount} we are now at state with {TickId}",
                clientWorld.EntityCount, playbackTick);
        }
    }
}