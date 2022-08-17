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
    public class ClientGhostPlayback
    {
        private readonly TimeTicker.TimeTicker ghostTicker;
        private readonly ILog log;
        private readonly SnapshotDeltaPackQueue queue = new();
        private readonly IEntityContainer entityWorld;
        private readonly IClientPredictorCorrections predictor;

        public ClientGhostPlayback(Milliseconds now, IEntityContainer entityWorld, IClientPredictorCorrections predictor, ILog log)
        {
            this.log = log;
            this.predictor = predictor;
            this.entityWorld = entityWorld;
            ghostTicker = new(now, GhostTick, new Milliseconds(16),
                log.SubLog("GhostPlaybackTick"));
        }

        public void Update(Milliseconds now)
        {
            ghostTicker.Update(now);

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
        
        private void GhostTick()
        {
            log.Debug("Ghost Tick!");
            if (queue.Count == 0)
            {
                log.Warn("Snapshot playback has stalled because queue is empty");
            }

            var deltaSnapshot = queue.Dequeue();
            var snapshotReader = new OctetReader(deltaSnapshot.payload);
            SnapshotDeltaReader.Read(snapshotReader, entityWorld);
            predictor.ReadCorrections(snapshotReader);

            // Our goal is to have just two snapshots in the queue.
            // So adjust the playback speed using the playback delta time.
            var deltaTimeMs = queue.Count switch
            {
                < 2 => 18,
                > 4 => 12,
                _ => 16
            };

            ghostTicker.DeltaTime = new Milliseconds(deltaTimeMs);
        }
    }
}