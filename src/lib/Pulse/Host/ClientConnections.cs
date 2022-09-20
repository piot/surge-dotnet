/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public sealed class ClientConnections
    {
        readonly Dictionary<uint, ConnectionToClient> connections = new();
        readonly ITransport hostTransport;
        readonly ILog log;
        readonly SnapshotSyncer notifySnapshotSyncer;
        readonly List<ConnectionToClient> orderedConnections = new();

        public ClientConnections(ITransport hostTransport, SnapshotSyncer notifySnapshotSyncer, ILog log)
        {
            this.hostTransport = hostTransport;
            this.notifySnapshotSyncer = notifySnapshotSyncer;
            this.log = log;
        }

        public IEnumerable<ConnectionToClient> Connections => orderedConnections;

        public void AssignPredictEntity(EndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            IEntity entity)
        {
            if (!connections.ContainsKey(connectionId.Value))
            {
                var snapshotSyncerClient = notifySnapshotSyncer.Create(connectionId);
                var preparedConnection = new ConnectionToClient(connectionId, snapshotSyncerClient,
                    log.SubLog($"Connection{connectionId.Value}"));
                //preparedConnection.AssignPredictEntityToPlayer(new LocalPlayerIndex(1), entity);
                connections[connectionId.Value] = preparedConnection;
                orderedConnections.Add(preparedConnection);
            }

            connections[connectionId.Value].AssignPredictEntityToPlayer(localPlayerIndex, entity);
        }

        public void ReceiveFromClients(TickId serverTickId)
        {
            for (var i = 0; i < 30; i++) // Have a maximum of datagrams to process each update, to avoid stalling
            {
                var datagram = hostTransport.Receive(out var clientId);
                if (datagram.IsEmpty)
                {
                    return;
                }

                if (!connections.TryGetValue(clientId.Value, out var connectionToClient))
                {
                    var syncer = notifySnapshotSyncer.Create(clientId);
                    connectionToClient =
                        new(clientId, syncer, log.SubLog($"Client/{clientId.Value}"));
                    orderedConnections.Add(connectionToClient);
                    connections.Add(clientId.Value, connectionToClient);
                }

                var datagramReader = new OctetReader(datagram.ToArray());
                connectionToClient.Receive(datagramReader, serverTickId);
            }
        }
    }
}