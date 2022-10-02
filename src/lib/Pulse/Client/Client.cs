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
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Surge.Types.Serialization;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Client
{
    public struct ClientInfo
    {
        public TimeMs now;
        public FixedDeltaTimeMs targetDeltaTimeMs;
        public IEntityContainerWithGhostCreator worldWithGhostCreator;
        public IEventProcessor eventProcessor;
        public ITransport assignedTransport;
        public IMultiCompressor compression;
        public IInputPackFetch fetch;
        public ISnapshotPlaybackNotify snapshotPlaybackNotify;
    }

    public sealed class Client : IOctetSerializable
    {
        readonly ClientPredictor clientPredictor;
        readonly ClientDatagramReceiver datagramReceiver;
        readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        readonly ClientLocalInputFetchAndSend localInputFetchAndSend;
        readonly ILog log;
        readonly TimeTicker statsTicker;
        readonly ITransportClient transportClient;
        readonly TransportStatsBoth transportWithStats;

        public Client(ClientInfo info, ILog log)
        {
            this.log = log;

            World = info.worldWithGhostCreator;

            transportWithStats = new(info.assignedTransport, info.now);
            transportClient = new TransportClient(transportWithStats);
            clientPredictor = new(log.SubLog("ClientPredictor"));
            const bool usePrediction = true;
            localInputFetchAndSend = new(info.fetch, clientPredictor, usePrediction,
                transportClient, info.now,
                info.targetDeltaTimeMs,
                info.worldWithGhostCreator,
                log.SubLog("InputFetchAndSend"));
            deltaSnapshotPlayback =
                new(info.now, info.worldWithGhostCreator, info.eventProcessor, localInputFetchAndSend,
                    info.snapshotPlaybackNotify, info.targetDeltaTimeMs,
                    log.SubLog("SnapshotPlayback"));

            datagramReceiver = new(transportClient, info.compression, deltaSnapshotPlayback,
                localInputFetchAndSend, log);
            statsTicker = new(new(0), StatsOutput, new(1000), log.SubLog("Stats"));
        }

        public bool UsePrediction
        {
            set => localInputFetchAndSend.UsePrediction = value;
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
            log.Info("Deserialize and reset");
            World.Reset();
            OctetMarker.AssertMarker(reader, 0xd7);
            var count = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < count; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var archetypeId = reader.ReadUInt16();
                var createdEntity = World.CreateGhostEntity(new(archetypeId), entityId);
                createdEntity.CompleteEntity.DeserializeAll(reader);
            }

            OctetMarker.AssertMarker(reader, 0x1a);
            datagramReceiver.Deserialize(reader);
            OctetMarker.AssertMarker(reader, 0xaf);
            deltaSnapshotPlayback.Deserialize(reader);
        }

        public void Serialize(IOctetWriter writer)
        {
            OctetMarker.WriteMarker(writer, 0xd7);
            EntityCountWriter.WriteEntityCount(World.EntityCount, writer);
            foreach (var entityToSerialize in World.AllEntities)
            {
                EntityIdWriter.Write(writer, entityToSerialize.Id);
                writer.WriteUInt16(entityToSerialize.ArchetypeId.id);
                entityToSerialize.CompleteEntity.SerializeAll(writer);
            }

            OctetMarker.WriteMarker(writer, 0x1a);
            datagramReceiver.Serialize(writer);
            OctetMarker.WriteMarker(writer, 0xaf);
            deltaSnapshotPlayback.Serialize(writer);
        }

        public EntityPredictor? FindPredictorFor(IEntity completeEntity)
        {
            return clientPredictor.FindPredictorFor(completeEntity);
        }

        void StatsOutput()
        {
            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", datagramReceiver.NetworkQuality);
        }

        public void ResetTime(TimeMs now)
        {
            localInputFetchAndSend.ResetTime(now);
            deltaSnapshotPlayback.ResetTime(now);
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