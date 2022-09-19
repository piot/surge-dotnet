/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Surge.Types.Serialization;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Client
{
    public sealed class Client : IOctetSerializable
    {
        private readonly ClientDatagramReceiver datagramReceiver;
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private readonly ClientLocalInputFetchAndSend localInputFetchAndSend;
        private readonly ILog log;
        private readonly TimeTicker statsTicker;
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;

        public Client(ILog log, TimeMs now, FixedDeltaTimeMs targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator, IEventProcessor eventProcessor,
            ITransport assignedTransport, IMultiCompressor compression, IInputPackFetch fetch,
            ISnapshotPlaybackNotify snapshotPlaybackNotify)
        {
            this.log = log;

            World = worldWithGhostCreator;

            transportWithStats = new(assignedTransport, now);
            transportClient = new TransportClient(transportWithStats);
            var clientPredictor = new ClientPredictor(log.SubLog("ClientPredictor"));
            const bool usePrediction = false;
            localInputFetchAndSend = new ClientLocalInputFetchAndSend(fetch, clientPredictor, usePrediction,
                transportClient, now,
                targetDeltaTimeMs,
                worldWithGhostCreator,
                log.SubLog("InputFetchAndSend"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, eventProcessor, localInputFetchAndSend,
                    snapshotPlaybackNotify, targetDeltaTimeMs,
                    log.SubLog("SnapshotPlayback"));

            datagramReceiver = new(transportClient, compression, deltaSnapshotPlayback, localInputFetchAndSend, log);
            statsTicker = new(new(0), StatsOutput, new(1000), log.SubLog("Stats"));
        }

        public ITransport Transport
        {
            get => transportWithStats.Transport;
            set => transportWithStats.Transport = value;
        }

        public bool ShouldApplyIncomingSnapshotsToWorld
        {
            get => deltaSnapshotPlayback.ShouldTickAndNotifySnapshots;
            set => deltaSnapshotPlayback.ShouldTickAndNotifySnapshots = value;
        }

        public TickId PlaybackTickId => deltaSnapshotPlayback.PlaybackTickId;

        public IInputPackFetch InputFetch
        {
            set => localInputFetchAndSend.InputFetch = value;
        }

        public LocalPlayersInfo LocalPlayersInfo => new(localInputFetchAndSend.LocalPlayerInputs);

        public IEnumerable<int> SnapshotLatencies => datagramReceiver.SnapshotLatencies;

        public IEntityContainerWithGhostCreator World { get; }

        public void Deserialize(IOctetReader reader)
        {
            var count = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < count; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var archetypeId = reader.ReadUInt16();
                var createdEntity = World.CreateGhostEntity(new(archetypeId), entityId);
                createdEntity.CompleteEntity.DeserializeAll(reader);
            }

            datagramReceiver.Deserialize(reader);
            deltaSnapshotPlayback.Deserialize(reader);
        }

        public void Serialize(IOctetWriter writer)
        {
            EntityCountWriter.WriteEntityCount(World.EntityCount, writer);
            foreach (var entityToSerialize in World.AllEntities)
            {
                EntityIdWriter.Write(writer, entityToSerialize.Id);
                writer.WriteUInt16(entityToSerialize.ArchetypeId.id);
                entityToSerialize.CompleteEntity.SerializeAll(writer);
            }

            datagramReceiver.Serialize(writer);
            deltaSnapshotPlayback.Serialize(writer);
        }

        private void StatsOutput()
        {
            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", datagramReceiver.NetworkQuality);
        }

        public void ResetTime(TimeMs now)
        {
            statsTicker.Reset(now);
        }

        public void Update(TimeMs now)
        {
            datagramReceiver.ReceiveDatagramsFromHost(now);
            transportWithStats.Update(now);
            localInputFetchAndSend.Update(now);
            deltaSnapshotPlayback.Update(now);
            statsTicker.Update(now);
        }
    }
}