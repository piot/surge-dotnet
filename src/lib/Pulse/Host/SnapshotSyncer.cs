/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Compress;
using Piot.Surge.Corrections;
using Piot.Surge.Corrections.Serialization;
using Piot.Surge.DeltaSnapshot.Cache;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
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
                ? new TickIdRange(connection.LastRemotelyProcessedTickId, serverTickId)
                : new TickIdRange(serverTickId, serverTickId);

            var wasFound = deltaSnapshotPackCache.FetchPack(rangeToSend, out var fetchedSnapshotPack);
            if (!wasFound)
            {
                var combinedMasks = entityMasksHistory.Fetch(rangeToSend);
            }

            var physicsCorrectionWriter = new OctetWriter(Constants.MaxSnapshotOctetSize);
            physicsCorrectionWriter.WriteUInt8((byte)connection.AssignedPredictedEntityForLocalPlayers.Keys.Count);

            foreach (var localPlayerAssignedPredictedEntity in connection.AssignedPredictedEntityForLocalPlayers)
            {
                var correctionsWrite = new OctetWriter(1024);
                var correctionEntity = localPlayerAssignedPredictedEntity.Value;
                correctionEntity.SerializeCorrectionState(correctionsWrite);

                CorrectionsHeaderWriter.Write(correctionEntity.Id,
                    new((byte)localPlayerAssignedPredictedEntity.Key), (ushort)correctionsWrite.Octets.Length,
                    physicsCorrectionWriter);
                physicsCorrectionWriter.WriteOctets(correctionsWrite.Octets);
            }

            var includingCorrections = new SnapshotDeltaPackIncludingCorrections(rangeToSend,
                fetchedSnapshotPack.payload.Span, physicsCorrectionWriter.Octets, fetchedSnapshotPack.PackType);

            var snapshotProtocolPack = SnapshotProtocolPacker.Pack(includingCorrections, compression, compressorIndex);

            var sender = new WrappedSender(transportSend, connection.Endpoint);

            log.DebugLowLevel("sending datagrams {Flattened}", includingCorrections);
            SnapshotPackIncludingCorrectionsWriter.Write(sender.Send, snapshotProtocolPack,
                connection.lastReceivedMonotonicTimeLowerBits, connection.clientInputTickCountAheadOfServer,
                serverTickId,
                connection.DatagramsSequenceIdIncrease);
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