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
    ///     includingCorrectionsQueue.
    /// </summary>
    public class ClientDeltaSnapshotPlayback
    {
        private readonly IEntityContainerWithGhostCreator clientWorld;
        private readonly IEventProcessor eventProcessor;
        private readonly SnapshotDeltaPackIncludingCorrectionsQueue includingCorrectionsQueue = new();

        private readonly HoldPositive lastBufferWasStarved = new(14);
        private readonly ILog log;
        private readonly IClientPredictorCorrections predictor;
        private readonly TimeTicker snapshotPlaybackTicker;
        private readonly Milliseconds targetDeltaTimeMs;
        private EventSequenceId expectedEventSequenceId;
        private TickId playbackTick = new(1);

        public ClientDeltaSnapshotPlayback(Milliseconds now, IEntityContainerWithGhostCreator clientWorld,
            IEventProcessor eventProcessor, IClientPredictorCorrections predictor,
            Milliseconds targetDeltaTimeMs, ILog log)
        {
            this.log = log;
            this.predictor = predictor;
            this.clientWorld = clientWorld;
            this.eventProcessor = eventProcessor;
            this.targetDeltaTimeMs = targetDeltaTimeMs;
            snapshotPlaybackTicker = new(now, NextSnapshotTick, targetDeltaTimeMs,
                log.SubLog("NextSnapshotTick"));
        }

        public bool LastPlaybackSnapshotWasSkipAhead { get; private set; }
        public bool LastPlaybackSnapshotWasMerged { get; private set; }
        public bool LastBufferWasStarved => lastBufferWasStarved.Value;

        public bool IsIncomingBufferStarving => lastBufferWasStarved.IsOrWasTrue;

        public void Update(Milliseconds now)
        {
            snapshotPlaybackTicker.Update(now);
        }

        public bool WantsSnapshotWithTickIdRange(TickIdRange tickIdRange)
        {
            return includingCorrectionsQueue.IsValidPackToInsert(tickIdRange);
        }

        public void FeedSnapshotDeltaPack(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!pack.tickIdRange.Contains(includingCorrectionsQueue.WantsTickId))
            {
                log.Notice("unexpected incoming delta pack encountered {TickIdRange}, expected {TickId}",
                    pack.tickIdRange, includingCorrectionsQueue.WantsTickId);
                return;
            }

            if (pack.tickIdRange.Last.tickId < includingCorrectionsQueue.WantsTickId.tickId)
            {
                log.Notice("old incoming delta pack encountered {TickIdRange}, expected {TickId}", pack.tickIdRange,
                    includingCorrectionsQueue.WantsTickId);
                return;
            }

            if (!includingCorrectionsQueue.IsValidPackToInsert(pack.tickIdRange))
            {
                throw new DeserializeException("wrong pack id ordering");
            }

            includingCorrectionsQueue.Enqueue(pack);

            if (includingCorrectionsQueue.LastInsertedIsMergedSnapshot)
            {
                log.Notice("Receiving merged snapshots {Range}", includingCorrectionsQueue.LastInsertedTickIdRange);
            }
        }

        private void NextSnapshotTick()
        {
            var targetDeltaTimeMsValue = targetDeltaTimeMs.ms;
            // Our goal is to have just two snapshots in the includingCorrectionsQueue.
            // So adjust the playback speed using the playback delta time.
            var bufferAheadCount = includingCorrectionsQueue.TicksAheadOf(playbackTick);
            var deltaTimeMs = bufferAheadCount switch
            {
                < 2 => targetDeltaTimeMsValue * 12 / 10,
                > 4 => targetDeltaTimeMsValue * 8 / 10,
                _ => targetDeltaTimeMsValue
            };

            log.DebugLowLevel("Try to read next snapshot in includingCorrectionsQueue. {PlaybackDeltaTimeMs}",
                deltaTimeMs);

            snapshotPlaybackTicker.DeltaTime = new(deltaTimeMs);

            lastBufferWasStarved.Value = bufferAheadCount < 2;

            if (includingCorrectionsQueue.Count == 0)
            {
                log.Notice(
                    "Snapshot playback has stalled because incoming snapshot includingCorrectionsQueue is empty");
                return;
            }

            playbackTick = new(playbackTick.tickId + 1);

            if (includingCorrectionsQueue.Peek().Pack.tickIdRange.Last > playbackTick)
            {
                log.Notice(
                    "Snapshot playback has stalled because next snapshot is in the future compared to playback");
                return;
            }

            var deltaSnapshotIncludingCorrectionsItem = includingCorrectionsQueue.Dequeue();

            var deltaSnapshotIncludingCorrections = deltaSnapshotIncludingCorrectionsItem.Pack;
            log.DebugLowLevel("dequeued snapshot {DeltaSnapshotEntityIds}", deltaSnapshotIncludingCorrections);

            var deltaSnapshotPack = new DeltaSnapshotPack(deltaSnapshotIncludingCorrections.tickIdRange,
                deltaSnapshotIncludingCorrections.deltaSnapshotPackPayload.Span,
                deltaSnapshotIncludingCorrections.StreamType, deltaSnapshotIncludingCorrections.SnapshotType);

            LastPlaybackSnapshotWasSkipAhead = deltaSnapshotIncludingCorrectionsItem.IsSkippedAheadSnapshot;
            LastPlaybackSnapshotWasMerged = deltaSnapshotIncludingCorrectionsItem.IsMergedAndOverlapping;

            expectedEventSequenceId = ApplyDeltaSnapshotToWorld.Apply(deltaSnapshotPack, clientWorld,
                eventProcessor, expectedEventSequenceId,
                deltaSnapshotIncludingCorrectionsItem.IsMergedAndOverlapping);

            predictor.ReadCorrections(deltaSnapshotIncludingCorrections.tickIdRange.Last,
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