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
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public class Host
    {
        private readonly ITransport transport;
        private readonly Dictionary<uint, ConnectionToClient> connections = new();
        private readonly List<ConnectionToClient> orderedConnections = new();
        private TickId serverTickId;
        private readonly TimeTicker.TimeTicker simulationTicker;
        private readonly ILog log;
        private readonly IEntityContainerWithChanges authoritativeWorld;
        private readonly DeltaSnapshotPackContainerHistory snapshotHistory = new();
        
        public Host(ITransport transport, ILog log)
        {
            authoritativeWorld = new AuthoritativeWorld();
            this.transport = transport;
            this.log = log;
            simulationTicker = new(new Milliseconds(0), SimulationTick, new Milliseconds(16),
                log.SubLog("SimulationTick"));
        }

        private void SendDeltaSnapshotToAllClients(DeltaSnapshotPackContainer container)
        {
            foreach (var connection in orderedConnections)
            {
                DeltaSnapshotPackContainer[] fetchedContainers;
                if (connection.WantsSnapshotFromTickId == 0)
                {
                    //serializeEverything
                }
                else if (connection.WantsResend)
                {
                    var tickIdRange = new TickIdRange(connection.ReSendFromTickId, serverTickId);
                    snapshotHistory.FetchContainers(tickIdRange);
                }
                else
                {
                    var onlyThisTick = new TickIdRange(serverTickId, serverTickId);
                    var containerForThisTick = snapshotHistory.FetchContainers(onlyThisTick);
                    
                }

                foreach (var fetchedContainer in fetchedContainers)
                {
                    var snapshotDeltaMemory = SnapshotPackContainerToMemory.PackWithFilter(fetchedContainer, Array.Empty<EntityId>());
                    var pack = SnapshotDeltaPacker.Pack(snapshotDeltaMemory);
                }

                SnapshotDeltaPackUnionToDatagramsWriter.Write(clientSender, unionFlattened,
                    connection.OrderedDatagrams);
            }
        }

        private void SimulationTick()
        {
            log.Debug("Simulation Tick!");
            serverTickId = new TickId(serverTickId.tickId + 1);
            var packContainer = StoreWorldChangesToPackContainer();
            SendDeltaSnapshotToAllClients(packContainer);
        }

        private DeltaSnapshotPackContainer StoreWorldChangesToPackContainer()
        {
            var deltaSnapshotInternal = SnapshotDeltaCreator.Scan(authoritativeWorld, serverTickId);
            var convertedDeltaSnapshot = FromSnapshotDeltaInternal.Convert(deltaSnapshotInternal);
            var deltaPackContainer = SnapshotDeltaPackCreator.Create(authoritativeWorld, convertedDeltaSnapshot);

            authoritativeWorld.ClearDelta();
            OverWriter.Overwrite(authoritativeWorld);

            return deltaPackContainer;
        }

        private void ReceiveFromClients()
        {
            for (var i = 0; i < 30; i++)
            {
                var datagram = transport.Receive(out var clientId);
                if (datagram.IsEmpty)
                {
                    return;
                }

                if (!connections.TryGetValue(clientId.Value, out var connectionToClient))
                {
                    connectionToClient = new ConnectionToClient(clientId);
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
        }
    }
}