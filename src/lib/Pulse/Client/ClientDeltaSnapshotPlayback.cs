/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.SnapshotProtocol.In;
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
        private readonly SnapshotDeltaPackIncludingCorrectionsQueue includingCorrectionsQueue = new();
        private readonly ILog log;
        private readonly IClientPredictorCorrections predictor;
        private readonly TimeTicker snapshotPlaybackTicker;
        private readonly Milliseconds targetDeltaTimeMs;

        public ClientDeltaSnapshotPlayback(Milliseconds now, IEntityContainerWithGhostCreator clientWorld,
            IClientPredictorCorrections predictor, Milliseconds targetDeltaTimeMs, ILog log)
        {
            this.log = log;
            this.predictor = predictor;
            this.clientWorld = clientWorld;
            this.targetDeltaTimeMs = targetDeltaTimeMs;
            snapshotPlaybackTicker = new(now, NextSnapshotTick, targetDeltaTimeMs,
                log.SubLog("NextSnapshotTick"));
        }

        public void Update(Milliseconds now)
        {
            snapshotPlaybackTicker.Update(now);
        }

        public void FeedSnapshotDeltaPack(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!pack.tickIdRange.Contains(includingCorrectionsQueue.WantsTickId))
            {
                return;
            }

            if (pack.tickIdRange.Last.tickId < includingCorrectionsQueue.WantsTickId.tickId)
            {
                return;
            }

            if (!includingCorrectionsQueue.IsValidPackToInsert(pack.tickIdRange.Last))
            {
                throw new DeserializeException("wrong pack id ordering");
            }

            includingCorrectionsQueue.Enqueue(pack);
        }

        private void NextSnapshotTick()
        {
            var targetDeltaTimeMsValue = targetDeltaTimeMs.ms;
            // Our goal is to have just two snapshots in the includingCorrectionsQueue.
            // So adjust the playback speed using the playback delta time.
            var deltaTimeMs = includingCorrectionsQueue.Count switch
            {
                < 2 => targetDeltaTimeMsValue * 10 / 8,
                > 4 => targetDeltaTimeMsValue * 10 / 15,
                _ => targetDeltaTimeMsValue
            };

            log.DebugLowLevel("Try to read next snapshot in includingCorrectionsQueue. {PlaybackDeltaTimeMs}",
                deltaTimeMs);

            snapshotPlaybackTicker.DeltaTime = new(deltaTimeMs);

            if (includingCorrectionsQueue.Count == 0)
            {
                log.Notice(
                    "Snapshot playback has stalled because incoming snapshot includingCorrectionsQueue is empty");
                return;
            }

            var deltaSnapshotIncludingCorrections = includingCorrectionsQueue.Dequeue();
            log.DebugLowLevel("dequeued snapshot {DeltaSnapshotEntityIds}", deltaSnapshotIncludingCorrections);

            var deltaSnapshotPack = new DeltaSnapshotPack(deltaSnapshotIncludingCorrections.tickIdRange,
                deltaSnapshotIncludingCorrections.deltaSnapshotPackPayload.Span,
                deltaSnapshotIncludingCorrections.PackType);

            ApplyDeltaSnapshotToWorld.Apply(deltaSnapshotPack, clientWorld);

            predictor.ReadCorrections(deltaSnapshotIncludingCorrections.tickIdRange.Last,
                deltaSnapshotIncludingCorrections.physicsCorrections.Span);

            // Ghosts are never predicted, corrected, nor rolled back
            // All the changed fields are set to the new values and Tick() is called to trigger the resulting effects of
            // the logic running for one tick.
            Ticker.Tick(clientWorld);
            log.DebugLowLevel("tick ghost logics {EntityCount}", clientWorld.AllEntities.Length);
        }
    }
}