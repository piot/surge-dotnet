/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Stats;
using Piot.Surge.Compress;
using Piot.Surge.DatagramType.Serialization;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.Fragment;
using Piot.Surge.SnapshotProtocol.In;
using Piot.Surge.Tick.Serialization;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Client
{
    public class Client
    {
        private readonly IMultiCompressor compression;
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private readonly HoldPositive isReceivingMergedSnapshots = new(20);
        private readonly ILog log;
        private readonly OrderedDatagramsInChecker orderedDatagramsInChecker = new();
        private readonly ClientPredictor predictor;
        private readonly SnapshotFragmentReAssembler snapshotFragmentReAssembler;
        private readonly StatCountThreshold statsHostInputQueueCount = new(60);
        private readonly StatCountThreshold statsRoundTripTime = new(20);
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;
        private readonly HoldPositive weAreSkippingAhead = new(25);

        public Client(ILog log, Milliseconds now, Milliseconds targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator, IEventProcessorWithCreate eventProcessorWithCreate,
            ITransport assignedTransport, IMultiCompressor compression, IInputPackFetch fetch)
        {
            this.log = log;
            this.compression = compression;
            snapshotFragmentReAssembler = new(log);
            World = worldWithGhostCreator;
            transportWithStats = new(assignedTransport, now);
            transportClient = new TransportClient(transportWithStats);
            predictor = new ClientPredictor(fetch, transportClient, now, targetDeltaTimeMs,
                worldWithGhostCreator,
                log.SubLog("Predictor"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, eventProcessorWithCreate, predictor,
                    targetDeltaTimeMs,
                    log.SubLog("GhostPlayback"));
        }

        public IInputPackFetch InputFetch
        {
            set => predictor.InputFetch = value;
        }

        public IEntityContainerWithGhostCreator World { get; }

        public ClientNetworkQuality NetworkQuality =>
            new()
            {
                isSkippingSnapshots = weAreSkippingAhead.IsOrWasTrue,
                isReceivingMergedSnapshots = isReceivingMergedSnapshots.IsOrWasTrue,
                isIncomingSnapshotPlaybackBufferStarving = deltaSnapshotPlayback.IsIncomingBufferStarving,
                averageRoundTripTimeMs = (uint)statsRoundTripTime.Stat.average
            };

        private void ReceiveSnapshotExtraData(IOctetReader reader, Milliseconds now)
        {
            var pongTimeLowerBits = MonotonicTimeLowerBitsReader.Read(reader);
            var pongTime = LowerBitsToMonotonic.LowerBitsToMonotonicMs(now, pongTimeLowerBits);
            var roundTripTimeMs = now.ms - pongTime.ms;
            statsRoundTripTime.Add((int)roundTripTimeMs);
            log.DebugLowLevel("RoundTripTime {RoundTripTimeMs} {AverageRoundTripTimeMs}", roundTripTimeMs,
                statsRoundTripTime.Stat.average);

            var numberOfInputInQueue = reader.ReadInt8();
            statsHostInputQueueCount.Add(numberOfInputInQueue);

            var serverIsProcessingTickId = TickIdReader.Read(reader);

            if (statsRoundTripTime.IsReady)
            {
                predictor.AdjustPredictionSpeed(serverIsProcessingTickId, (uint)statsRoundTripTime.Stat.average);
            }

            log.DebugLowLevel("InputQueueCountFromHost {InputQueueCount} {AverageInputQueueCount}",
                numberOfInputInQueue, statsHostInputQueueCount.Stat.average);
        }

        private void ReceiveSnapshot(IOctetReader reader, Milliseconds now)
        {
            log.DebugLowLevel("receiving snapshot datagram from server");
            ReceiveSnapshotExtraData(reader, now);
            var snapshotIsDone = snapshotFragmentReAssembler.Read(reader, out var tickIdRange, out var completePayload);
            predictor.LastSeenSnapshotTickId = tickIdRange.Last;

            if (!snapshotIsDone)
            {
                return;
            }

            if (!deltaSnapshotPlayback.WantsSnapshotWithTickIdRange(tickIdRange))
            {
                return;
            }

            var snapshotWithCorrections =
                DeltaSnapshotIncludingCorrectionsReader.Read(tickIdRange, completePayload, compression);

            deltaSnapshotPlayback.FeedSnapshotDeltaPack(snapshotWithCorrections);

            weAreSkippingAhead.Value = deltaSnapshotPlayback.LastPlaybackSnapshotWasSkipAhead;
            if (weAreSkippingAhead.IsOrWasTrue)
            {
                log.Notice("we are or have been skipping ahead!");
            }

            isReceivingMergedSnapshots.Value = deltaSnapshotPlayback.LastPlaybackSnapshotWasMerged;
            if (isReceivingMergedSnapshots.Value)
            {
                log.Notice("we are receiving merged snapshots");
            }

            predictor.NextExpectedSnapshotTickId = snapshotWithCorrections.tickIdRange.lastTickId.Next();
        }

        private void ReceiveDatagramFromHost(IOctetReader reader, Milliseconds now)
        {
            if (!orderedDatagramsInChecker.ReadAndCheck(reader))
            {
                log.Notice("ordered datagram in wrong order, discarding datagram {OrderedDatagramsSequenceId}",
                    orderedDatagramsInChecker);
                return;
            }

            var datagramType = DatagramTypeReader.Read(reader);
            switch (datagramType)
            {
                case DatagramType.DatagramType.DeltaSnapshots:
                    ReceiveSnapshot(reader, now);
                    break;
                default:
                    throw new DeserializeException($"illegal datagram type {datagramType} from host");
            }
        }

        private void ReceiveDatagramsFromHost(Milliseconds now)
        {
            for (var i = 0; i < 30; i++)
            {
                var datagram = transportClient.ReceiveFromHost();
                if (datagram.IsEmpty)
                {
                    return;
                }

                var datagramReader = new OctetReader(datagram);
                ReceiveDatagramFromHost(datagramReader, now);
            }
        }

        public void Update(Milliseconds now)
        {
            ReceiveDatagramsFromHost(now);
            transportWithStats.Update(now);
            predictor.Update(now);
            deltaSnapshotPlayback.Update(now);
            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", NetworkQuality);
        }
    }
}