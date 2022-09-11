/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.Compress;
using Piot.Surge.Corrections;
using Piot.Surge.Corrections.Serialization;
using Piot.Surge.DeltaSnapshot.Convert;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.Event;
using Piot.Surge.SnapshotProtocol.Out;
using Piot.Surge.Tick;
using Piot.Transport;
using Constants = Piot.Surge.SnapshotProtocol.Constants;

namespace Piot.Surge.Pulse.Host
{
    public class SnapshotSyncer
    {
        private readonly IMultiCompressor compression;
        private readonly CompressorIndex compressorIndex;
        private readonly EntityMasksHistory entityMasksHistory = new();
        private readonly ILog log;
        private readonly List<SnapshotSyncerClient> syncClients = new();
        private readonly ITransportSend transportSend;

        public SnapshotSyncer(ITransportSend transportSend, IMultiCompressor compression,
            CompressorIndex compressorIndex, ILog log)
        {
            this.compression = compression;
            this.compressorIndex = compressorIndex;
            this.transportSend = transportSend;
            this.log = log;
        }

        public SnapshotSyncerClient Create(RemoteEndpointId id)
        {
            var client = new SnapshotSyncerClient(id);

            syncClients.Add(client);

            return client;
        }

        private DeltaSnapshotPack FindBestPack(SnapshotSyncerClient connection,
            DeltaSnapshotPack precalculatedPackForThisTick, IEntityContainer world, EventStreamPackQueue eventStream,
            TickId serverTickId)
        {
            if (connection.WantsResend)
            {
                var rangeToSend = new TickIdRange(connection.RemoteIsExpectingTickId, serverTickId);
                var combinedMasks = entityMasksHistory.Fetch(rangeToSend);

                var snapshotEntityIds = EntityMasksToDeltaSnapshotIds.ToEntityIds(combinedMasks);
                var eventsForThisRange = eventStream.FetchEventsForRange(rangeToSend);

                return DeltaSnapshotToBitPack.ToDeltaSnapshotPack(world, eventsForThisRange, snapshotEntityIds,
                    rangeToSend);
            }

            if (!connection.HasReceivedInitialState)
            {
                // Send Complete State
                var octets = CompleteStateBitWriter.CaptureCompleteSnapshotPack(world, eventStream.NextSequenceId);
                return new(new(new(0), serverTickId), octets, SnapshotStreamType.BitStream, SnapshotType.CompleteState);
            }

            if (precalculatedPackForThisTick.tickIdRange.Last == serverTickId &&
                precalculatedPackForThisTick.tickIdRange.Length == 1)
            {
                return precalculatedPackForThisTick;
            }

            throw new Exception("internal error when finding best pack");
        }

        private void SendUsingContainers(SnapshotSyncerClient connection,
            DeltaSnapshotPack precalculatedPackForThisTick, IEntityContainer world, EventStreamPackQueue eventStream,
            TickId hostTickId)
        {
            var bestPack = FindBestPack(connection, precalculatedPackForThisTick, world, eventStream, hostTickId);

            var physicsCorrectionWriter = new OctetWriter(Constants.MaxSnapshotOctetSize);
            physicsCorrectionWriter.WriteUInt8((byte)connection.AssignedPredictedEntityForLocalPlayers.Keys.Count);

            foreach (var localPlayerAssignedPredictedEntity in connection.AssignedPredictedEntityForLocalPlayers)
            {
                var correctionsWrite = new OctetWriter(1024);
                var correctionEntity = localPlayerAssignedPredictedEntity.Value;
                correctionEntity.CompleteEntity.SerializeCorrectionState(correctionsWrite);

                CorrectionsHeaderWriter.Write(correctionEntity.Id,
                    new((byte)localPlayerAssignedPredictedEntity.Key), (ushort)correctionsWrite.Octets.Length,
                    physicsCorrectionWriter);
                physicsCorrectionWriter.WriteOctets(correctionsWrite.Octets);
            }

            var includingCorrections = new SnapshotDeltaPackIncludingCorrections(bestPack.tickIdRange,
                bestPack.payload.Span, physicsCorrectionWriter.Octets, bestPack.StreamType, bestPack.SnapshotType);

            var snapshotProtocolPack = SnapshotProtocolPacker.Pack(includingCorrections, compression, compressorIndex);

            var sender = new WrappedSender(transportSend, connection.Endpoint);

            log.DebugLowLevel("sending datagrams {Flattened}", includingCorrections);
            SnapshotPackIncludingCorrectionsWriter.Write(sender.Send, snapshotProtocolPack,
                connection.lastReceivedMonotonicTimeLowerBits, connection.clientInputTickCountAheadOfServer,
                hostTickId,
                connection.DatagramsSequenceIdIncrease);
        }

        public void SendSnapshot(EntityMasks masks, DeltaSnapshotPack deltaSnapshotPack, IEntityContainer world,
            EventStreamPackQueue eventStream)
        {
            entityMasksHistory.Enqueue(masks);
            foreach (var syncClient in syncClients)
            {
                SendUsingContainers(syncClient, deltaSnapshotPack, world, eventStream,
                    deltaSnapshotPack.tickIdRange.Last);
            }
        }
    }
}