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
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.Fragment;
using Piot.Surge.SnapshotProtocol.In;
using Piot.Surge.Tick.Serialization;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    public class ClientDatagramReceiver
    {
        private readonly IMultiCompressor compression;
        private readonly HoldPositive isReceivingMergedSnapshots = new(20);
        private readonly ILog log;
        private readonly ClientLocalInputFetch notifyLocalInputFetch;
        private readonly ClientDeltaSnapshotPlayback notifyPlayback;
        private readonly OrderedDatagramsInChecker orderedDatagramsInChecker = new();
        private readonly SnapshotFragmentReAssembler snapshotFragmentReAssembler;
        private readonly StatCountThreshold statsHostInputQueueCount = new(60);
        private readonly StatCountThreshold statsRoundTripTime = new(10);
        private readonly ITransportClient transportClient;
        private readonly HoldPositive weAreSkippingAhead = new(25);

        public ClientDatagramReceiver(ITransportClient transportClient, IMultiCompressor compression,
            ClientDeltaSnapshotPlayback notifyPlayback, ClientLocalInputFetch notifyLocalInputFetch, ILog log)
        {
            this.log = log;
            this.compression = compression;
            this.notifyPlayback = notifyPlayback;
            this.notifyLocalInputFetch = notifyLocalInputFetch;
            this.transportClient = transportClient;
            snapshotFragmentReAssembler = new(log);
        }

        public ClientNetworkQuality NetworkQuality =>
            new()
            {
                isSkippingSnapshots = weAreSkippingAhead.IsOrWasTrue,
                isReceivingMergedSnapshots = isReceivingMergedSnapshots.IsOrWasTrue,
                isIncomingSnapshotPlaybackBufferStarving = notifyPlayback.IsIncomingBufferStarving,
                averageRoundTripTimeMs = (uint)statsRoundTripTime.Stat.average
            };

        private void ReceiveSnapshotExtraData(IOctetReader reader, Milliseconds now)
        {
            var pongTimeLowerBits = MonotonicTimeLowerBitsReader.Read(reader);
            var pongTime = LowerBitsToMonotonic.LowerBitsToMonotonicMs(now, pongTimeLowerBits);
            var roundTripTimeMs = now.ms - pongTime.ms;
            statsRoundTripTime.Add((int)roundTripTimeMs);
            log.Debug("RoundTripTime {Now} {PongTime} {RoundTripTimeMs} {AverageRoundTripTimeMs}", now.ms, pongTime.ms,
                roundTripTimeMs,
                statsRoundTripTime.Stat.average);

            var numberOfInputInQueue = reader.ReadInt8();
            statsHostInputQueueCount.Add(numberOfInputInQueue);

            var serverIsProcessingTickId = TickIdReader.Read(reader);


            log.DebugLowLevel("InputQueueCountFromHost {InputQueueCount} {AverageInputQueueCount}",
                numberOfInputInQueue, statsHostInputQueueCount.Stat.average);
        }

        private void ReceiveSnapshot(IOctetReader reader, Milliseconds now)
        {
            log.DebugLowLevel("receiving snapshot datagram from server");
            ReceiveSnapshotExtraData(reader, now);
            var snapshotIsDone = snapshotFragmentReAssembler.Read(reader, out var tickIdRange, out var completePayload);
            notifyLocalInputFetch.LastSeenSnapshotTickId = tickIdRange.Last;

            notifyLocalInputFetch.AdjustInputTickSpeed(tickIdRange.Last,
                (uint)statsRoundTripTime.Stat.average);

            if (!snapshotIsDone)
            {
                return;
            }

            if (!notifyPlayback.WantsSnapshotWithTickIdRange(tickIdRange))
            {
                return;
            }

            var snapshotWithCorrections =
                DeltaSnapshotIncludingCorrectionsReader.Read(tickIdRange, completePayload, compression);

            notifyPlayback.FeedSnapshotDeltaPack(snapshotWithCorrections);

            weAreSkippingAhead.Value = notifyPlayback.LastPlaybackSnapshotWasSkipAhead;
            if (weAreSkippingAhead.IsOrWasTrue)
            {
                log.Notice("we are or have been skipping ahead!");
            }

            isReceivingMergedSnapshots.Value = notifyPlayback.LastPlaybackSnapshotWasMerged;
            if (isReceivingMergedSnapshots.Value)
            {
                log.Notice("we are receiving merged snapshots");
            }

            notifyLocalInputFetch.NextExpectedSnapshotTickId = snapshotWithCorrections.tickIdRange.lastTickId.Next();
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

        public void ReceiveDatagramsFromHost(Milliseconds now)
        {
            for (var i = 0; i < 5; i++)
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
    }
}