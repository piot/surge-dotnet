/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge.Pulse.Client
{
    /// <summary>
    ///     Enqueues delta snapshots and plays them back at varying delta time depending on number of delta snapshots in queue.
    /// </summary>
    public class ClientDeltaSnapshotPlayback
    {
        private readonly IEntityContainerWithCreation clientWorld;
        private readonly ILog log;
        private readonly IClientPredictorCorrections predictor;
        private readonly SnapshotDeltaPackQueue queue = new();
        private readonly TimeTicker.TimeTicker snapshotPlaybackTicker;
        private readonly Milliseconds targetDeltaTimeMs;

        public ClientDeltaSnapshotPlayback(Milliseconds now, IEntityContainerWithCreation clientWorld,
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

        public void FeedSnapshotsUnion(SerializedSnapshotDeltaPackUnion snapshots)
        {
            if (!snapshots.tickIdRange.Contains(queue.WantsTickId))
            {
                return;
            }

            foreach (var pack in snapshots.packs)
            {
                if (pack.tickId.tickId < queue.WantsTickId.tickId)
                {
                    continue;
                }

                if (!queue.IsValidPackToInsert(pack.tickId))
                {
                    throw new DeserializeException("wrong pack id ordering");
                }

                queue.Enqueue(pack);
            }
        }

        private void NextSnapshotTick()
        {
            var targetDeltaTimeMsValue = targetDeltaTimeMs.ms;
            // Our goal is to have just two snapshots in the queue.
            // So adjust the playback speed using the playback delta time.
            var deltaTimeMs = queue.Count switch
            {
                < 2 => targetDeltaTimeMsValue * 10 / 8,
                > 4 => targetDeltaTimeMsValue * 10 / 15,
                _ => targetDeltaTimeMsValue
            };

            log.DebugLowLevel("Try to read next snapshot in queue. {PlaybackDeltaTimeMs}", deltaTimeMs);

            snapshotPlaybackTicker.DeltaTime = new(deltaTimeMs);

            if (queue.Count == 0)
            {
                log.Notice("Snapshot playback has stalled because incoming snapshot queue is empty");
                return;
            }

            var deltaSnapshot = queue.Dequeue();
            log.DebugLowLevel("dequeued snapshot {DeltaSnapshot}", deltaSnapshot);
            var snapshotReader = new OctetReader(deltaSnapshot.payload.Span);
            SnapshotDeltaReader.Read(snapshotReader, clientWorld);

            // Ghosts are never predicted, corrected, nor rolled back
            // All the changed fields are set to the new values and Tick() is called to trigger the resulting effects of
            // the logic running for one tick.
            Ticker.Tick(clientWorld);
            log.DebugLowLevel("tick ghost logics {EntityCount}", clientWorld.AllEntities.Length);

            predictor.ReadCorrections(deltaSnapshot.tickId, snapshotReader);
        }
    }
}