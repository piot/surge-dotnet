/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Collections;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Stats;
using Piot.Surge.Compress;
using Piot.Surge.DatagramType.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.Fragment;
using Piot.Surge.SnapshotProtocol.In;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    public sealed class ClientDatagramReceiver
    {
        readonly IMultiCompressor compression;
        readonly HoldPositive isReceivingMergedSnapshots = new(20);
        readonly ILog log;
        readonly ClientLocalInputFetchAndSend notifyLocalInputFetchAndSend;
        readonly ClientDeltaSnapshotPlayback notifyPlayback;
        readonly Action<TickId> onFirstSnapshot;
        readonly OrderedDatagramsInChecker orderedDatagramsInChecker = new();
        readonly SnapshotFragmentReAssembler snapshotFragmentReAssembler;
        readonly CircularBuffer<int> snapshotLatencies = new(128);
        readonly StatCountThreshold statsHostInputQueueCount = new(60);
        readonly StatCountThreshold statsRoundTripTime = new(10);
        readonly ITransportClient transportClient;
        readonly HoldPositive weAreSkippingAhead = new(25);
        bool hasReceivedFirstSnapshot;
        long lastReceivedRoundTripTimeMs;

        public ClientDatagramReceiver(ITransportClient transportClient, IMultiCompressor compression,
            ClientDeltaSnapshotPlayback notifyPlayback, Action<TickId> onFirstSnapshot,
            ClientLocalInputFetchAndSend notifyLocalInputFetchAndSend,
            ILog log)
        {
            this.log = log;
            this.onFirstSnapshot = onFirstSnapshot;
            this.compression = compression;
            this.notifyPlayback = notifyPlayback;
            this.notifyLocalInputFetchAndSend = notifyLocalInputFetchAndSend;
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

        public IEnumerable<int> SnapshotLatencies => snapshotLatencies;

        long ReceiveSnapshotExtraData(IOctetReader reader, TimeMs now)
        {
            var snapshotExtraBits = reader.ReadUInt8();
            long roundTripTimeMs = 0;
            if ((snapshotExtraBits & 0x01) == 0x01)
            {
                var pongTimeLowerBits = MonotonicTimeLowerBitsReader.Read(reader);
                var pongTime = LowerBitsToMonotonic.LowerBitsToMonotonicMs(now, pongTimeLowerBits);
                roundTripTimeMs = now.ms - pongTime.ms;
                lastReceivedRoundTripTimeMs = roundTripTimeMs;
                statsRoundTripTime.Add((int)roundTripTimeMs);
                log.Debug("RoundTripTime {Now} {PongTime} {RoundTripTimeMs} {AverageRoundTripTimeMs}", now.ms,
                    pongTime.ms,
                    roundTripTimeMs,
                    statsRoundTripTime.Stat.average);
            }
            else
            {
                roundTripTimeMs = lastReceivedRoundTripTimeMs;
            }

            var numberOfInputInQueue = reader.ReadInt8();
            statsHostInputQueueCount.Add(numberOfInputInQueue);

            var serverIsProcessingTickId = TickIdReader.Read(reader);

            log.DebugLowLevel("InputQueueCountFromHost {InputQueueCount} {AverageInputQueueCount}",
                numberOfInputInQueue, statsHostInputQueueCount.Stat.average);

            return roundTripTimeMs;
        }

        public void Serialize(IOctetWriter writer)
        {
            orderedDatagramsInChecker.Serialize(writer);
            snapshotFragmentReAssembler.Serialize(writer);
        }

        public void Deserialize(IOctetReader reader)
        {
            orderedDatagramsInChecker.Deserialize(reader);
            snapshotFragmentReAssembler.Deserialize(reader);
        }

        void ReceiveSnapshot(IOctetReader reader, TimeMs now)
        {
            log.DebugLowLevel("receiving snapshot datagram from server");
            var lastRoundTripTime = ReceiveSnapshotExtraData(reader, now);
            var snapshotState = snapshotFragmentReAssembler.Read(reader, out var tickIdRange, out var completePayload);
            notifyLocalInputFetchAndSend.LastSeenSnapshotTickId = tickIdRange.Last;

            if (snapshotState != SnapshotFragmentReAssembler.State.Done)
            {
                return;
            }

            snapshotLatencies.Enqueue((int)lastRoundTripTime);

            notifyLocalInputFetchAndSend.AdjustInputTickSpeed(tickIdRange.Last,
                (uint)statsRoundTripTime.Stat.average);

            if (!notifyPlayback.WantsSnapshotWithTickIdRange(tickIdRange))
            {
                return;
            }


            if (!hasReceivedFirstSnapshot)
            {
                hasReceivedFirstSnapshot = true;
                onFirstSnapshot(tickIdRange.Last);
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

            notifyLocalInputFetchAndSend.NextExpectedSnapshotTickId =
                snapshotWithCorrections.tickIdRange.lastTickId.Next;
        }

        void ReceiveDatagramFromHost(IOctetReader reader, TimeMs now)
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

        public void ReceiveDatagramsFromHost(TimeMs now)
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