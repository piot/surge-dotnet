/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

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

        public Host(ITransport hostTransport, IMultiCompressor compression, CompressorIndex compressorIndex,
            IEntityContainerWithDetectChanges world, TimeMs now, ILog log)
        {
            transportWithStats = new(hostTransport, now);
            transport = transportWithStats;
            snapshotSyncer = new(transport, compression, compressorIndex, log.SubLog("SnapshotSyncer"));
            AuthoritativeWorld = world;
            clientConnections = new(hostTransport, snapshotSyncer, log);
            this.log = log;
            simulationTicker = new(new(0), SimulationTick, new(16),
                log.SubLog("SimulationTick"));
            statsTicker = new(new(0), StatsOutput, new(1000), log.SubLog("Stats"));
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
            Ticker.Tick(AuthoritativeWorld);
            Ticker.TickMovementSimulation(AuthoritativeWorld);
            Notifier.Notify(AuthoritativeWorld.AllEntities);
        }

        public void AssignPredictEntity(EndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            IEntity entity)
        {
            clientConnections.AssignPredictEntity(connectionId, localPlayerIndex, entity);
        }

        void SimulationTick()
        {
            //log.Debug("== Simulation Tick! {TickId}", authoritativeTickId);

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