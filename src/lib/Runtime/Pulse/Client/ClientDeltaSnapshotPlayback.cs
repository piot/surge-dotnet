/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Stats;
using Piot.Surge.Core;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;
using Piot.Surge.SnapshotProtocol.In;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;
using Piot.Surge.TimeTick;

namespace Piot.Surge.Pulse.Client
{
    /// <summary>
    ///     Enqueues delta snapshots and plays them back at varying delta time depending on number of delta snapshots in
    ///     snapshotsQueue.
    /// </summary>
    public sealed class ClientDeltaSnapshotPlayback : IOctetSerializable
    {

        public delegate void SnapshotPlaybackDelegate(TimeMs now, TickId hostTick, DeltaSnapshotPack tickId);
        readonly IDataReceiver clientWorld;
        readonly IEventReceiver eventProcessor;

        readonly HoldPositive lastBufferWasStarved = new(14);
        readonly ILog log;
        readonly IClientPredictorCorrections predictor;
        readonly SnapshotPlaybackDelegate snapshotPlaybackNotify;
        readonly TimeTicker snapshotPlaybackTicker;
        readonly SnapshotDeltaPackQueue snapshotsQueue = new();
        EventSequenceId expectedEventSequenceId;
        TickId playbackTick = new(0);
        FixedDeltaTimeMs targetDeltaTimeMs;

        public ClientDeltaSnapshotPlayback(TimeMs now, IDataReceiver clientWorld,
            IEventReceiver eventProcessor, IClientPredictorCorrections predictor,
            SnapshotPlaybackDelegate snapshotPlaybackNotify,
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

        public bool ShouldTickAndNotifySnapshots { get; set; } = true;
        public bool shouldUseDequeuedSnapshots { get; set; } = true;

        public bool IsIncomingBufferStarving => lastBufferWasStarved.IsOrWasTrue;

        public TickId PlaybackTickId
        {
            get => playbackTick;

            set => playbackTick = value;
        }

        public void Deserialize(IOctetReader reader)
        {
            snapshotsQueue.Deserialize(reader);
            targetDeltaTimeMs = new(reader.ReadUInt32());
            expectedEventSequenceId = EventSequenceIdReader.Read(reader);
            playbackTick = TickIdReader.Read(reader);
        }

        public void Serialize(IOctetWriter writer)
        {
            snapshotsQueue.Serialize(writer);
            writer.WriteUInt32(targetDeltaTimeMs.ms);
            EventSequenceIdWriter.Write(writer, expectedEventSequenceId);
            TickIdWriter.Write(writer, playbackTick);
        }

        public void ResetTime(TimeMs now)
        {
            snapshotPlaybackTicker.Reset(now);
        }

        public void Update(TimeMs now)
        {
            snapshotPlaybackTicker.Update(now);
        }

        public bool WantsSnapshotWithTickIdRange(TickIdRange tickIdRange)
        {
            return snapshotsQueue.IsValidPackToInsert(tickIdRange);
        }

        public void FeedSnapshotDeltaPack(DeltaSnapshotPack pack)
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

        void NextSnapshotTick()
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
                // log.Notice(
                //   "Snapshot playback has stalled because incoming snapshot snapshotsQueue is empty");
                return;
            }

            playbackTick = new(playbackTick.tickId + 1);

            if (snapshotsQueue.Peek().tickIdRange.Last > playbackTick)
            {
                if (ShouldTickAndNotifySnapshots)
                {
                    log.Notice(
                        "Snapshot playback has stalled because next snapshot is in the future compared to playback");
                }

                return;
            }

            var deltaSnapshotIncludingPredictAssignmentHeader = snapshotsQueue.Dequeue();

            log.DebugLowLevel("dequeued snapshot {DeltaSnapshotEntityIds}", deltaSnapshotIncludingPredictAssignmentHeader);

            LastPlaybackSnapshotWasSkipAhead = false; // TODO: deltaSnapshotIncludingCorrectionsItem.IsSkippedAheadSnapshot;
            LastPlaybackSnapshotWasMerged = false; // TODO: deltaSnapshotIncludingCorrections.tickIdRange.IsOverlappingAndMerged;

            if (!shouldUseDequeuedSnapshots)
            {
                return;
            }

            var isMergedAndOverlapping = false; // TODO: deltaSnapshotIncludingCorrectionsItem.IsMergedAndOverlapping

            var deltaSnapshotPack = new DeltaSnapshotPack(deltaSnapshotIncludingPredictAssignmentHeader.tickIdRange,
                deltaSnapshotIncludingPredictAssignmentHeader.payload.Span, deltaSnapshotIncludingPredictAssignmentHeader.SnapshotType);

            snapshotPlaybackNotify?.Invoke(snapshotPlaybackTicker.Now, playbackTick, deltaSnapshotPack);

            // TODO: Serialize exact bit count
            var bitSnapshotReader =
                new BitReader(deltaSnapshotPack.payload.Span, deltaSnapshotPack.payload.Length * 8);
            
            try
            {
                log.Debug("Playback {Snapshot}", deltaSnapshotPack);
                expectedEventSequenceId = ApplyDeltaSnapshotToWorld.Apply(bitSnapshotReader, deltaSnapshotPack.SnapshotType, clientWorld,
                    eventProcessor, expectedEventSequenceId,
                    isMergedAndOverlapping, log);
            }
            catch (DeserializeException e)
            {
                log.Notice(e.Message);
            }

            predictor.ReadPredictEntityIdsForLocalPlayers(bitSnapshotReader);

            if (!ShouldTickAndNotifySnapshots)
            {
                // Just to save some performance of Tick and Notify for a client running on a host
                return;
            }
            
            

            log.DebugLowLevel("tick ghost logics. we are now at state with {TickId}",
                playbackTick);
        }
    }
}