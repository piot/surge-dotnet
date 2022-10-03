/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Transport;
using Piot.Transport.Stats;
using Constants = Piot.Surge.SnapshotProtocol.Constants;

namespace Piot.Surge.Pulse.Host
{
    public struct HostInfo
    {
        public ITransport hostTransport;
        public IMultiCompressor compression;
        public CompressorIndex compressorIndex;
        public IEntityContainerWithDetectChanges authoritativeWorld;
        public TimeMs now;
        public Action<ConnectionToClient> onConnectionCreated;
        public FixedDeltaTimeMs targetDeltaTimeMs;
    }

    public sealed class Host
    {
        readonly BitWriter cachedSnapshotBitWriter = new(Constants.MaxSnapshotOctetSize);
        readonly OctetWriter cachedSnapshotWriter = new(Constants.MaxSnapshotOctetSize);
        readonly ClientConnections clientConnections;
        readonly ILog log;
        readonly TimeTicker simulationTicker;
        readonly SnapshotSyncer snapshotSyncer;
        readonly TimeTicker statsTicker;
        readonly ITransport transport;
        readonly TransportStatsBoth transportWithStats;
        TickId authoritativeTickId;

        public Host(HostInfo info, ILog log)
        {
            transportWithStats = new(info.hostTransport, info.now);
            transport = transportWithStats;
            snapshotSyncer = new(transport, info.compression, info.compressorIndex, log.SubLog("SnapshotSyncer"));
            AuthoritativeWorld = info.authoritativeWorld;
            clientConnections = new(info.hostTransport, snapshotSyncer, info.onConnectionCreated, log);
            this.log = log;
            simulationTicker = new(info.now, SimulationTick, info.targetDeltaTimeMs,
                log.SubLog("SimulationTick"));
            statsTicker = new(info.now, StatsOutput, new(1000), log.SubLog("Stats"));
            ShortLivedEventStream = new(authoritativeTickId);
        }

        /// <summary>
        ///     Short Lived Events are things that are short lived and forgettable in nature (less than one second of lifetime)
        ///     They can be skipped for late-joining, re-joining or re-syncing clients without any impact.
        ///     Usually used for effects like minor projectile explosions.
        /// </summary>
        public EventStreamPackQueue ShortLivedEventStream { get; }

        public IEntityContainerWithDetectChanges AuthoritativeWorld { get; }

        public TransportStats Stats => transportWithStats.Stats;

        void TickWorld()
        {
            foreach (var entity in AuthoritativeWorld.AllEntities)
            {
                entity.CompleteEntity.CaptureSnapshot();
                entity.CompleteEntity.Tick();
                entity.CompleteEntity.MovementSimulationTick();
                Notifier.Notify(entity);
            }
        }

        public void AssignPredictEntity(EndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            IEntity entity)
        {
            clientConnections.AssignPredictEntity(connectionId, localPlayerIndex, entity);
        }

        void SimulationTick()
        {
            log.DebugLowLevel("== Simulation Tick! {TickId}", authoritativeTickId);

            TickWorld();

            authoritativeTickId = authoritativeTickId.Next;
            ShortLivedEventStream.EndOfTick(authoritativeTickId);
            //log.Debug("== Simulation Tick post! {TickId}", authoritativeTickId);

            SetInputFromClients.SetInputsFromClientsToEntities(clientConnections.Connections, authoritativeTickId, log);

            cachedSnapshotBitWriter.Reset();
            cachedSnapshotWriter.Reset();
            var (masks, deltaSnapshotPack) = StoreWorldChanges.StoreWorldChangesToPackContainer(AuthoritativeWorld,
                ShortLivedEventStream, authoritativeTickId, SnapshotStreamType.BitStream, cachedSnapshotWriter,
                cachedSnapshotBitWriter);
            snapshotSyncer.SendSnapshot(masks, deltaSnapshotPack, AuthoritativeWorld, ShortLivedEventStream);

            foreach (var entity in AuthoritativeWorld.AllEntities)
            {
                entity.CompleteEntity.ClearChanges();
            }
        }

        void StatsOutput()
        {
            log.DebugLowLevel("stats: {Stats}", transportWithStats.Stats);
        }

        public void ResetTime(TimeMs now)
        {
            simulationTicker.Reset(now);
            statsTicker.Reset(now);
        }

        public void Update(TimeMs now)
        {
            clientConnections.ReceiveFromClients(authoritativeTickId);
            simulationTicker.Update(now);
            transportWithStats.Update(now);
            statsTicker.Update(now);
        }
    }
}