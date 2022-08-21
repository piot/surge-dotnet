/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.SnapshotSerialization;
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
        private readonly DeltaSnapshotPackContainerHistory snapshotHistory = new();
        private readonly List<SnapshotSyncerClient> syncClients = new();
        private readonly ITransportSend transportSend;

        public SnapshotSyncer(ITransportSend transportSend)
        {
            this.transportSend = transportSend;
        }

        public SnapshotSyncerClient Create(RemoteEndpointId id)
        {
            var client = new SnapshotSyncerClient(id);

            syncClients.Add(client);

            return client;
        }

        private void SendUsingContainers(SnapshotSyncerClient connection, TickId serverTickId)
        {
            var rangeToSend = connection.WantsResend
                ? connection.WaitingForTickIds
                : new TickIdRange(serverTickId, serverTickId);

            var fetchedContainers = snapshotHistory.FetchContainers(rangeToSend);

            List<SnapshotDeltaPack.SnapshotDeltaPack> deltaPacks = new();
            foreach (var fetchedContainer in fetchedContainers)
            {
                var snapshotDeltaMemory =
                    SnapshotPackContainerToMemory.PackWithFilter(fetchedContainer, Array.Empty<EntityId>());
                var packWithCorrections = SnapshotDeltaPacker.Pack(snapshotDeltaMemory);
                var deltaPack = new SnapshotDeltaPack.SnapshotDeltaPack(fetchedContainer.TickId, packWithCorrections);
                deltaPacks.Add(deltaPack);
            }

            var serializedUnion = new SerializedSnapshotDeltaPackUnion
            {
                tickIdRange = rangeToSend,
                packs = deltaPacks.ToArray()
            };

            var sender = new WrappedSender(transportSend, connection.Endpoint);

            var unionFlattened = SnapshotDeltaUnionPacker.Pack(serializedUnion);

            SnapshotDeltaPackUnionToDatagramsWriter.Write(sender.Send, unionFlattened,
                connection.lastReceivedMonotonicTimeLowerBits, connection.clientInputTickCountAheadOfServer,
                connection.DatagramsOutIncrease);
        }

        public void SendSnapshot(DeltaSnapshotPackContainer container)
        {
            snapshotHistory.Add(container);
            foreach (var syncClient in syncClients)
            {
                SendUsingContainers(syncClient, container.TickId);
            }
        }
    }
}