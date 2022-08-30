/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Corrections;
using Piot.Surge.DeltaSnapshot.Cache;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Entities;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.SnapshotProtocol.Out;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public enum SnapshotSyncerClientState
    {
        Normal,
        Resend
    }

    public class SnapshotSyncerClient
    {
        public sbyte clientInputTickCountAheadOfServer;
        public MonotonicTimeLowerBits.MonotonicTimeLowerBits lastReceivedMonotonicTimeLowerBits;
        private SnapshotSyncerClientState state = SnapshotSyncerClientState.Normal;

        public TickIdRange WaitingForTickIds;

        public SnapshotSyncerClient(RemoteEndpointId id)
        {
            Endpoint = id;
        }

        public Entity? AssignedPredictEntity { get; set; }

        public RemoteEndpointId Endpoint { get; }
        public OrderedDatagramsOutIncrease DatagramsOutIncrease { get; } = new();
        public bool WantsResend => false;
    }

    public class WrappedSender
    {
        private readonly RemoteEndpointId id;
        private readonly ITransportSend sender;

        public WrappedSender(ITransportSend sender, RemoteEndpointId id)
        {
            this.sender = sender;
            this.id = id;
        }

        public void Send(ReadOnlyMemory<byte> datagram)
        {
            sender.SendToEndpoint(id, datagram.Span);
        }
    }

    public class SnapshotSyncer
    {
        private readonly DeltaSnapshotPackCache deltaSnapshotPackCache;
        private readonly EntityMasksHistory entityMasksHistory = new();
        private readonly ILog log;
        private readonly List<SnapshotSyncerClient> syncClients = new();
        private readonly ITransportSend transportSend;

        public SnapshotSyncer(ITransportSend transportSend, ILog log)
        {
            deltaSnapshotPackCache = new(log);
            this.transportSend = transportSend;
            this.log = log;
        }

        public SnapshotSyncerClient Create(RemoteEndpointId id)
        {
            var client = new SnapshotSyncerClient(id);

            syncClients.Add(client);

            return client;
        }

        private void SendUsingContainers(SnapshotSyncerClient connection, ConnectionPlayer[] connectionPlayers,
            TickId serverTickId)
        {
            var rangeToSend = connection.WantsResend
                ? connection.WaitingForTickIds
                : new TickIdRange(serverTickId, serverTickId);

            var wasFound = deltaSnapshotPackCache.FetchPack(rangeToSend, out var fetchedContainer);
            if (!wasFound)
            {
                var combinedMasks = entityMasksHistory.Fetch(rangeToSend);
            }

            SnapshotDeltaPackIncludingCorrections includingCorrections;
            if (connectionPlayers.Length > 0)
            {
                /*
                var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);
                writer.WriteOctets(fetchedContainer.payload.Span);
                writer.WriteUInt8((byte)connectionPlayers.Length);

                foreach (var predicted in connectionPlayers)
                {
                }

                includingCorrections = new(rangeToSend, writer.Octets);
                */
            }

            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);
            writer.WriteOctets(fetchedContainer.payload.Span);
            writer.WriteUInt16(0); // HACK: for now, no corrections

            includingCorrections = new(rangeToSend, writer.Octets);

            var snapshotProtocolPack = SnapshotProtocolPacker.Pack(includingCorrections);

            var sender = new WrappedSender(transportSend, connection.Endpoint);

            log.DebugLowLevel("sending datagrams {Flattened}", fetchedContainer);
            SnapshotPackIncludingCorrectionsWriter.Write(sender.Send, snapshotProtocolPack,
                connection.lastReceivedMonotonicTimeLowerBits, connection.clientInputTickCountAheadOfServer,
                serverTickId,
                connection.DatagramsOutIncrease);
        }

        private void SendUsingMask(SnapshotSyncerClient connection)
        {
            var maskUnion = entityMasksHistory.Fetch(connection.WaitingForTickIds);
        }

        public void SendSnapshot(EntityMasks masks, ConnectionPlayer[] connectionPlayers,
            DeltaSnapshotPack deltaSnapshotPack)
        {
            deltaSnapshotPackCache.Add(deltaSnapshotPack);
            entityMasksHistory.Enqueue(masks);
            foreach (var syncClient in syncClients)
            {
                SendUsingContainers(syncClient, connectionPlayers, deltaSnapshotPack.tickIdRange.Last);
            }
        }
    }
}