/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Compress;
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
        private readonly IMultiCompressor compress;
        private readonly RemoteEndpointId id;
        private readonly ITransportSend sender;

        public WrappedSender(ITransportSend sender, IMultiCompressor compress, RemoteEndpointId id)
        {
            this.compress = compress;
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
        private readonly IMultiCompressor compression;
        private readonly CompressorIndex compressorIndex;
        private readonly DeltaSnapshotPackCache deltaSnapshotPackCache;
        private readonly EntityMasksHistory entityMasksHistory = new();
        private readonly ILog log;
        private readonly List<SnapshotSyncerClient> syncClients = new();
        private readonly ITransportSend transportSend;

        public SnapshotSyncer(ITransportSend transportSend, IMultiCompressor compression,
            CompressorIndex compressorIndex, ILog log)
        {
            this.compression = compression;
            this.compressorIndex = compressorIndex;
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

        private void SendUsingContainers(SnapshotSyncerClient connection, TickId serverTickId)
        {
            var rangeToSend = connection.WantsResend
                ? connection.WaitingForTickIds
                : new TickIdRange(serverTickId, serverTickId);

            var wasFound = deltaSnapshotPackCache.FetchPack(rangeToSend, out var fetchedSnapshotPack);
            if (!wasFound)
            {
                var combinedMasks = entityMasksHistory.Fetch(rangeToSend);
            }

            var physicsCorrectionWriter = new OctetWriter(Constants.MaxSnapshotOctetSize);
            physicsCorrectionWriter.WriteUInt8(0); // connection.localPlayerAssignedPredictedEntities
            /*
            foreach (var predicted in connection.localPlayerAssignedPredictedEntities)
            {
            }
            */

            var includingCorrections = new SnapshotDeltaPackIncludingCorrections(rangeToSend,
                fetchedSnapshotPack.payload.Span, physicsCorrectionWriter.Octets, fetchedSnapshotPack.PackType);

            var snapshotProtocolPack = SnapshotProtocolPacker.Pack(includingCorrections, compression, compressorIndex);

            var sender = new WrappedSender(transportSend, compression, connection.Endpoint);

            log.DebugLowLevel("sending datagrams {Flattened}", includingCorrections);
            SnapshotPackIncludingCorrectionsWriter.Write(sender.Send, snapshotProtocolPack,
                connection.lastReceivedMonotonicTimeLowerBits, connection.clientInputTickCountAheadOfServer,
                serverTickId,
                connection.DatagramsOutIncrease);
        }

        private void SendUsingMask(SnapshotSyncerClient connection)
        {
            var maskUnion = entityMasksHistory.Fetch(connection.WaitingForTickIds);
        }

        public void SendSnapshot(EntityMasks masks, DeltaSnapshotPack deltaSnapshotPack)
        {
            deltaSnapshotPackCache.Add(deltaSnapshotPack);
            entityMasksHistory.Enqueue(masks);
            foreach (var syncClient in syncClients)
            {
                SendUsingContainers(syncClient, deltaSnapshotPack.tickIdRange.Last);
            }
        }
    }
}