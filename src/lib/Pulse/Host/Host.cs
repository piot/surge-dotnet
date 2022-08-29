/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDeltaInternal;
using Piot.Surge.SnapshotDeltaMasks;
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.TransportStats;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public class Host
    {
        //private readonly AuthoritativeWorld authoritativeWorld;
        private readonly Dictionary<uint, ConnectionToClient> connections = new();
        private readonly ILog log;
        private readonly List<ConnectionToClient> orderedConnections = new();
        private readonly TimeTicker.TimeTicker simulationTicker;
        private readonly SnapshotSyncer snapshotSyncer;
        private readonly ITransport transport;
        private readonly TransportStatsBoth transportWithStats;
        private TickId serverTickId;

        public Host(ITransport hostTransport, IEntityContainerWithDetectChanges world, Milliseconds now, ILog log)
        {
            transportWithStats = new TransportStatsBoth(hostTransport, now);
            transport = transportWithStats;
            snapshotSyncer = new SnapshotSyncer(transport, log.SubLog("Syncer"));
            AuthoritativeWorld = world;
            this.log = log;
            simulationTicker = new(new Milliseconds(0), SimulationTick, new Milliseconds(16),
                log.SubLog("SimulationTick"));
        }

        public IEntityContainerWithDetectChanges AuthoritativeWorld { get; }

        public TransportStats.TransportStats Stats => transportWithStats.Stats;

        private void SetInputsFromClientsToEntities()
        {
            foreach (var connection in orderedConnections)
            {
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
                    if (!connection.HasAssignedEntity)
                    {
                        continue;
                    }

                    var targetEntity = AuthoritativeWorld.FetchEntity(connection.ControllingEntityId);
                    var inputDeserialize = targetEntity.GeneratedEntity as IInputDeserialize;
                    if (inputDeserialize is null)
                    {
                        throw new Exception(
                            $"It is not possible to control Entity {connection.ControllingEntityId}, it has no IDeserializeInput interface");
                    }

                    var inputReader = new OctetReader(input.payload.Span);
                    inputDeserialize.SetInput(inputReader);
                }
            }
        }

        private void TickWorld()
        {
            Ticker.Tick(AuthoritativeWorld);
        }

        private void SimulationTick()
        {
            log.Debug("== Simulation Tick! {TickId}", serverTickId);
            SetInputsFromClientsToEntities();
            TickWorld();
            var (masks, packContainer) = StoreWorldChangesToPackContainer();
            snapshotSyncer.SendSnapshot(masks, packContainer);
            serverTickId = new TickId(serverTickId.tickId + 1);
        }

        private (SnapshotDeltaEntityMasks, DeltaSnapshotPackContainer) StoreWorldChangesToPackContainer()
        {
            var snapshotDelta = SnapshotDeltaCreator.Scan(AuthoritativeWorld, serverTickId);
            var deltaPackContainer =
                SnapshotDeltaPackCreator.Create(AuthoritativeWorld, snapshotDelta);

            AuthoritativeWorld.ClearDelta();
            OverWriter.Overwrite(AuthoritativeWorld);

            return (SnapshotDeltaToEntityMasks.ConvertToEntityMasks(snapshotDelta), deltaPackContainer);
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