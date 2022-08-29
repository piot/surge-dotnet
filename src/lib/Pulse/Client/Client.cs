/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Stats;
using Piot.Surge.DatagramType;
using Piot.Surge.LogicalInput;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotSerialization;
using Piot.Surge.TransportStats;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    public class Client
    {
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private readonly ILog log;
        private readonly OrderedDatagramsInChecker orderedDatagramsInChecker = new();
        private readonly ClientPredictor predictor;
        private readonly StatCountThreshold statsHostInputQueueCount = new(60);

        private readonly StatCountThreshold statsRoundTripTime = new(20);
        private readonly ITransport transportBoth;
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;

        public Client(ILog log, Milliseconds now, Milliseconds targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator,
            ITransport assignedTransport, IInputPackFetch fetch)
        {
            this.log = log;
            World = worldWithGhostCreator;
            transportWithStats = new(assignedTransport, now);
            transportBoth = transportWithStats;
            transportClient = new TransportClient(transportBoth);
            predictor = new ClientPredictor(fetch, transportClient, now, targetDeltaTimeMs, worldWithGhostCreator,
                log.SubLog("Predictor"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, predictor, targetDeltaTimeMs,
                    log.SubLog("GhostPlayback"));
        }

        public IEntityContainerWithGhostCreator World { get; }

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
            SnapshotPackDatagramHeaderReader.Read(reader, out var tickIdRange, out var datagramIndex,
                out var isLastOne);
            log.DebugLowLevel("receive snapshot header {TickIdRange} {DatagramIndex} {IsLastOne}", tickIdRange,
                datagramIndex, isLastOne);
            var unionOfSnapshots = SnapshotDeltaUnionReader.Read(reader);
            deltaSnapshotPlayback.FeedSnapshotDeltaPack(unionOfSnapshots);
        }

        private void ReceiveDatagramFromHost(IOctetReader reader, Milliseconds now)
        {
            if (!orderedDatagramsInChecker.ReadAndCheck(reader))
            {
                log.Notice("ordered datagram in wrong order, discarding datagram {OrderedDatagramsIn}",
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
        }
    }
}