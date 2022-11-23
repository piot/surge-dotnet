/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.Compress;
using Piot.Surge.Corrections.Serialization;
using Piot.Surge.DeltaSnapshot.ComponentFieldMask;
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
        readonly BitWriter cachedBitWriterForSnapshots = new(Constants.MaxSnapshotOctetSize);
        readonly OctetWriter cachedCompleteCompressedDeltaSnapshotPackWriter = new(12 * 1024);
        readonly OctetWriter cachedCompressionWriter = new(12 * 1024);
        readonly OctetWriter cachedDatagramsWriter = new(Transport.Constants.MaxDatagramOctetSize);
        readonly OctetWriter cachedSinglePredictionHeaderWriter = new(1024);
        readonly OctetWriter cachedSnapshotWithPredictionHeaderWriter = new(10 * 1024);
        readonly IMultiCompressor compression;
        readonly CompressorIndex compressorIndex;
        readonly ILog log;
        readonly List<SnapshotSyncerClient> syncClients = new();
        readonly ITransportSend transportSend;
        TickId allClientsAreWaitingForAtLeastTickId;

        public SnapshotSyncer(ITransportSend transportSend, IMultiCompressor compression,
            CompressorIndex compressorIndex, ILog log)
        {
            this.compression = compression;
            this.compressorIndex = compressorIndex;
            this.transportSend = transportSend;
            this.log = log;
        }


        public AllEntitiesChangesEachTickHistory History { get; } = new();

        void HandleNotifyExpectedTickId(TickId _)
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
            History.DiscardUpTo(allClientsAreWaitingForAtLeastTickId);
        }

        public SnapshotSyncerClient Create(EndpointId id)
        {
            var client = new SnapshotSyncerClient(id, HandleNotifyExpectedTickId);

            syncClients.Add(client);

            return client;
        }

        DeltaSnapshotPack CreateDeltaSnapshotPack(SnapshotSyncerClient connection,
            IDataSender world, EventStreamPackQueue eventStream,
            TickId serverTickId)
        {
            if (!connection.HasReceivedInitialState)
            {
                cachedBitWriterForSnapshots.Reset();
                // Send Complete State
                var octets = CompleteStateBitWriter.CaptureCompleteSnapshotPack(world, connection.AssignedPredictedEntityIdValuesForLocalPlayers, eventStream.NextSequenceId, cachedBitWriterForSnapshots, log);
                return new(new(new(0), serverTickId), octets, SnapshotType.CompleteState);
            }

            // In most cases, just send the delta that happened this tick
            var rangeToSend = TickIdRange.FromTickId(serverTickId);
            if (connection.WantsResend)
            {
                rangeToSend = new(connection.RemoteIsExpectingTickId, serverTickId);
            }

            var combinedMasks = History.Fetch(rangeToSend, log);
            log.Debug("fetch {CombinedMasks}", combinedMasks);

            var eventsForThisRange = eventStream.FetchEventsForRange(rangeToSend);
            if (eventsForThisRange.Length > 0)
            {
                log.DebugLowLevel("fetched event packs for {TickId} to {EndTickId}", eventsForThisRange[0],
                    eventsForThisRange[^1]);
            }

            cachedBitWriterForSnapshots.Reset();

            var clientSidePredictedEntities = connection.AssignedPredictedEntityForLocalPlayers.Values.Select(x => (uint)x.Value).ToArray();
            return DeltaSnapshotToBitPack.ToDeltaSnapshotPack(world, eventsForThisRange, clientSidePredictedEntities, combinedMasks,
                rangeToSend, cachedBitWriterForSnapshots, log);
        }

        void SendUsingContainers(SnapshotSyncerClient connection,
            IDataSender world, EventStreamPackQueue eventStream,
            TickId hostTickId)
        {
            var deltaSnapshotPack = CreateDeltaSnapshotPack(connection, world, eventStream, hostTickId);

            log.Debug("created raw {Snapshot}", deltaSnapshotPack);

            log.DebugLowLevel("Sending assigned physics correction {LocalPlayerCount}",
                connection.AssignedPredictedEntityForLocalPlayers.Keys.Count);

            cachedSnapshotWithPredictionHeaderWriter.Reset();
            cachedSnapshotWithPredictionHeaderWriter.WriteUInt8((byte)connection.AssignedPredictedEntityForLocalPlayers.Keys
                .Count);

            foreach (var localPlayerAssignedPredictedEntity in connection.AssignedPredictedEntityForLocalPlayers)
            {
                cachedSinglePredictionHeaderWriter.Reset();
                var correctionEntity = localPlayerAssignedPredictedEntity.Value;
                // TODO: correctionEntity.CompleteEntity.SerializeCorrectionState(cachedSinglePhysicsCorrectionWriter);

                ClientPredictorHeaderWriter.Write(correctionEntity,
                    new((byte)localPlayerAssignedPredictedEntity.Key),
                    cachedSnapshotWithPredictionHeaderWriter);
                cachedSnapshotWithPredictionHeaderWriter.WriteOctets(cachedSinglePredictionHeaderWriter.Octets);
            }


            cachedCompleteCompressedDeltaSnapshotPackWriter.Reset();
            cachedCompressionWriter.Reset();
            DeltaSnapshotWithHeaderAndCompressionWriter.Write(deltaSnapshotPack, compression, compressorIndex,
                cachedCompleteCompressedDeltaSnapshotPackWriter, cachedCompressionWriter);
            var snapshotProtocolPack =
                new SnapshotProtocolPack(deltaSnapshotPack.tickIdRange, cachedCompleteCompressedDeltaSnapshotPackWriter.Octets);
            var sender = new WrappedSender(transportSend, connection.Endpoint);

            log.Debug("sending datagrams {SnapshotWithPredictionInfo} {Compression} {ClientPontTime}", deltaSnapshotPack, compressorIndex,
                connection.lastReceivedMonotonicTimeLowerBits);

            cachedDatagramsWriter.Reset();
            SnapshotProtocolPackToDatagramsWriter.Write(sender.Send, snapshotProtocolPack,
                connection.lastReceivedMonotonicTimeLowerBits, connection.hasLastReceivedMonotonicTimeLowerBits,
                connection.clientInputTickCountAheadOfServer,
                hostTickId,
                connection.DatagramsSequenceIdIncrease, cachedDatagramsWriter);
            connection.hasLastReceivedMonotonicTimeLowerBits = false;
        }

        public void SendSnapshotToClients(IDataSender dataSender,
            EventStreamPackQueue eventStream, TickId hostTickId)
        {
            foreach (var syncClient in syncClients)
            {
                SendUsingContainers(syncClient, dataSender, eventStream, hostTickId);
            }
        }
    }
}