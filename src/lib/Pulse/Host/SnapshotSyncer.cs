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
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.SnapshotProtocol.Out;
using Piot.Surge.Tick;
using Piot.Transport;
using Constants = Piot.Surge.SnapshotProtocol.Constants;

namespace Piot.Surge.Pulse.Host
{
    public sealed class SnapshotSyncer
    {
        private readonly BitWriter cachedBitWriterForSnapshots = new(Constants.MaxSnapshotOctetSize);
        private readonly OctetWriter cachedDatagramsWriter = new(Constants.MaxDatagramOctetSize);
        private readonly OctetWriter cachedPhysicsCorrectionPackSubWriter = new(12 * 1024);
        private readonly OctetWriter cachedPhysicsCorrectionPackWriter = new(12 * 1024);
        private readonly OctetWriter cachedPhysicsCorrectionWriter = new(10 * 1024);
        private readonly OctetWriter cachedSinglePhysicsCorrectionWriter = new(1024);
        private readonly IMultiCompressor compression;
        private readonly CompressorIndex compressorIndex;
        private readonly EntityMasksHistory entityMasksHistory = new();
        private readonly ILog log;
        private readonly List<SnapshotSyncerClient> syncClients = new();
        private readonly ITransportSend transportSend;
        private TickId allClientsAreWaitingForAtLeastTickId;


        public SnapshotSyncer(ITransportSend transportSend, IMultiCompressor compression,
            CompressorIndex compressorIndex, ILog log)
        {
            this.compression = compression;
            this.compressorIndex = compressorIndex;
            this.transportSend = transportSend;
            this.log = log;
        }

        private void HandleNotifyExpectedTickId(TickId _)
        {
            TickId lowestTickId = new(0);
            var hasBeenSet = false;
            foreach (var syncClient in syncClients)
            {
                if (syncClient.RemoteIsExpectingTickId <= allClientsAreWaitingForAtLeastTickId)
                {
                    // We can give up early, this client is still waiting for the same tick id
                    return;
                }

                if (!hasBeenSet || syncClient.RemoteIsExpectingTickId < lowestTickId)
                {
                    lowestTickId = syncClient.RemoteIsExpectingTickId;
                    hasBeenSet = true;
                }
            }

            if (!hasBeenSet || lowestTickId < allClientsAreWaitingForAtLeastTickId)
            {
                return;
            }

            allClientsAreWaitingForAtLeastTickId = lowestTickId;
            entityMasksHistory.DiscardUpTo(allClientsAreWaitingForAtLeastTickId);
        }

        public SnapshotSyncerClient Create(RemoteEndpointId id)
        {
            var client = new SnapshotSyncerClient(id, HandleNotifyExpectedTickId);

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
                if (eventsForThisRange.Length > 0)
                {
                    log.DebugLowLevel("fetched event packs for {TickId} to {EndTickId}", eventsForThisRange[0],
                        eventsForThisRange[^1]);
                }

                cachedBitWriterForSnapshots.Reset();

                return DeltaSnapshotToBitPack.ToDeltaSnapshotPack(world, eventsForThisRange, snapshotEntityIds,
                    rangeToSend, cachedBitWriterForSnapshots);
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

            cachedPhysicsCorrectionWriter.Reset();
            cachedPhysicsCorrectionWriter.WriteUInt8((byte)connection.AssignedPredictedEntityForLocalPlayers.Keys
                .Count);

            foreach (var localPlayerAssignedPredictedEntity in connection.AssignedPredictedEntityForLocalPlayers)
            {
                cachedSinglePhysicsCorrectionWriter.Reset();
                var correctionEntity = localPlayerAssignedPredictedEntity.Value;
                correctionEntity.CompleteEntity.SerializeCorrectionState(cachedSinglePhysicsCorrectionWriter);

                CorrectionsHeaderWriter.Write(correctionEntity.Id,
                    new((byte)localPlayerAssignedPredictedEntity.Key),
                    (ushort)cachedSinglePhysicsCorrectionWriter.Octets.Length,
                    cachedPhysicsCorrectionWriter);
                cachedPhysicsCorrectionWriter.WriteOctets(cachedSinglePhysicsCorrectionWriter.Octets);
            }

            var includingCorrections = new SnapshotDeltaPackIncludingCorrections(bestPack.tickIdRange,
                bestPack.payload.Span, cachedPhysicsCorrectionWriter.Octets, bestPack.StreamType,
                bestPack.SnapshotType);

            cachedPhysicsCorrectionPackWriter.Reset();
            cachedPhysicsCorrectionPackSubWriter.Reset();
            SnapshotIncludingCorrectionsWriter.Write(includingCorrections, compression, compressorIndex,
                cachedPhysicsCorrectionPackWriter, cachedPhysicsCorrectionPackSubWriter);
            var snapshotProtocolPack =
                new SnapshotProtocolPack(bestPack.tickIdRange, cachedPhysicsCorrectionPackWriter.Octets);
            var sender = new WrappedSender(transportSend, connection.Endpoint);

            log.Debug("sending datagrams {Flattened} {ClientPontTime}", includingCorrections,
                connection.lastReceivedMonotonicTimeLowerBits);

            cachedDatagramsWriter.Reset();
            SnapshotPackIncludingCorrectionsWriter.Write(sender.Send, snapshotProtocolPack,
                connection.lastReceivedMonotonicTimeLowerBits, connection.clientInputTickCountAheadOfServer,
                hostTickId,
                connection.DatagramsSequenceIdIncrease, cachedDatagramsWriter);
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