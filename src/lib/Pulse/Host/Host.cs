/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.DeltaSnapshot.Convert;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.DeltaSnapshot.Scan;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Host
{
    public class Host
    {
        private readonly IMultiCompressor compression;

        private readonly Dictionary<uint, ConnectionToClient> connections = new();
        private readonly ILog log;
        private readonly List<ConnectionToClient> orderedConnections = new();
        private readonly bool shouldUseBitStream = true;
        private readonly TimeTicker simulationTicker;
        private readonly SnapshotSyncer snapshotSyncer;
        private readonly ITransport transport;
        private readonly TransportStatsBoth transportWithStats;
        private TickId serverTickId;

        public Host(ITransport hostTransport, IMultiCompressor compression, CompressorIndex compressorIndex,
            IEntityContainerWithDetectChanges world, Milliseconds now, ILog log)
        {
            this.compression = compression;
            transportWithStats = new TransportStatsBoth(hostTransport, now);
            transport = transportWithStats;
            snapshotSyncer = new SnapshotSyncer(transport, compression, compressorIndex, log.SubLog("Syncer"));
            AuthoritativeWorld = world;
            this.log = log;
            simulationTicker = new(new Milliseconds(0), SimulationTick, new Milliseconds(16),
                log.SubLog("SimulationTick"));
        }

        public IEntityContainerWithDetectChanges AuthoritativeWorld { get; }

        public TransportStats Stats => transportWithStats.Stats;

        public void AssignPredictEntity(RemoteEndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            IEntity entity)
        {
            if (!connections.ContainsKey(connectionId.Value))
            {
                var snapshotSyncerClient = snapshotSyncer.Create(connectionId);
                var preparedConnection = new ConnectionToClient(connectionId, snapshotSyncerClient,
                    log.SubLog($"Connection{connectionId.Value}"));
                //preparedConnection.AssignPredictEntityToPlayer(new LocalPlayerIndex(1), entity);
                connections[connectionId.Value] = preparedConnection;
                orderedConnections.Add(preparedConnection);
            }

            connections[connectionId.Value].AssignPredictEntityToPlayer(localPlayerIndex, entity);
        }

        private void SetInputsFromClientsToEntities()
        {
            foreach (var connection in orderedConnections)
            {
                log.DebugLowLevel("checking inputs from connection {Connection}", connection);
                foreach (var connectionPlayer in connection.ConnectionPlayers.Values)
                {
                    var logicalInputQueue = connectionPlayer.LogicalInputQueue;
                    if (!logicalInputQueue.HasInputForTickId(serverTickId))
                    {
                        // The old data on the input is intentionally kept
                        log.Notice($"connection {connection.Id} didn't have an input for tick {serverTickId}");
                        continue;
                    }

                    var input = logicalInputQueue.Dequeue();
                    log.DebugLowLevel("dequeued logical input {ConnectionPlayer} {Input}", connectionPlayer, input);

                    {
                        var targetEntity =
                            connectionPlayer
                                .AssignedPredictEntity;
                        if (targetEntity is null)
                        {
                            log.Notice("target entity is null, can not apply input");
                            continue;
                        }

                        if (targetEntity.GeneratedEntity is not IInputDeserialize inputDeserialize)
                        {
                            throw new Exception(
                                $"It is not possible to control Entity {targetEntity}, it has no IDeserializeInput interface");
                        }

                        var inputReader = new OctetReader(input.payload.Span);
                        log.DebugLowLevel("setting input for {Entity}", targetEntity);
                        inputDeserialize.SetInput(inputReader);
                    }
                }
            }
        }

        private void TickWorld()
        {
            Ticker.Tick(AuthoritativeWorld);
            Notifier.Notify(AuthoritativeWorld.AllEntities);
        }

        private void SimulationTick()
        {
            log.Debug("== Simulation Tick! {TickId}", serverTickId);

            TickWorld();
            SetInputsFromClientsToEntities();

            var nextTickId = new TickId(serverTickId.tickId + 1);
            // Exactly after Tick() and the input has been set in preparation for the next tick, we mark this as the new tick
            serverTickId = nextTickId;
            log.Debug("== Simulation Tick post! {TickId}", serverTickId);

            var (masks, deltaSnapshotPack) = StoreWorldChangesToPackContainer();
            snapshotSyncer.SendSnapshot(masks, deltaSnapshotPack, AuthoritativeWorld);
        }

        private (EntityMasks, DeltaSnapshotPack) StoreWorldChangesToPackContainer()
        {
            var deltaSnapshotEntityIds = Scanner.Scan(AuthoritativeWorld, serverTickId);
            var deltaSnapshotPack = shouldUseBitStream
                ? DeltaSnapshotToBitPack.ToDeltaSnapshotPack(AuthoritativeWorld, deltaSnapshotEntityIds,
                    TickIdRange.FromTickId(deltaSnapshotEntityIds.TickId))
                : DeltaSnapshotToPack.ToDeltaSnapshotPack(AuthoritativeWorld, deltaSnapshotEntityIds);

            AuthoritativeWorld.ClearDelta();
            OverWriter.Overwrite(AuthoritativeWorld);

            return (DeltaSnapshotToEntityMasks.ToEntityMasks(deltaSnapshotEntityIds), deltaSnapshotPack);
        }

        private void ReceiveFromClients()
        {
            for (var i = 0; i < 30; i++) // Have a maximum of datagrams to process each update, to avoid stalling
            {
                var datagram = transport.Receive(out var clientId);
                if (datagram.IsEmpty)
                {
                    return;
                }


                if (!connections.TryGetValue(clientId.Value, out var connectionToClient))
                {
                    var syncer = snapshotSyncer.Create(clientId);
                    connectionToClient =
                        new ConnectionToClient(clientId, syncer, log.SubLog($"Client{clientId.Value}"));
                    orderedConnections.Add(connectionToClient);
                    connections.Add(clientId.Value, connectionToClient);
                }


                var datagramReader = new OctetReader(datagram.ToArray());
                connectionToClient.Receive(datagramReader, serverTickId);
            }
        }

        public void Update(Milliseconds now)
        {
            ReceiveFromClients();
            simulationTicker.Update(now);
            transportWithStats.Update(now);
            log.DebugLowLevel("stats: {Stats}", transportWithStats.Stats);
        }
    }
}