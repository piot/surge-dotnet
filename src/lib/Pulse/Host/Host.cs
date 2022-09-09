/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.DeltaSnapshot.Convert;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.DeltaSnapshot.Scan;
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Host
{
    public class Host
    {
        private readonly ILog log;

        private readonly bool shouldUseBitStream = true;
        private readonly TimeTicker simulationTicker;
        private readonly SnapshotSyncer snapshotSyncer;
        private readonly ITransport transport;
        private readonly TransportStatsBoth transportWithStats;
        private readonly ClientConnections clientConnections;
        private TickId serverTickId;

        public Host(ITransport hostTransport, IMultiCompressor compression, CompressorIndex compressorIndex,
            IEntityContainerWithDetectChanges world, Milliseconds now, ILog log)
        {
            transportWithStats = new TransportStatsBoth(hostTransport, now);
            transport = transportWithStats;
            snapshotSyncer = new SnapshotSyncer(transport, compression, compressorIndex, log.SubLog("Syncer"));
            AuthoritativeWorld = world;
            clientConnections = new(hostTransport, snapshotSyncer, log);
            this.log = log;
            simulationTicker = new(new Milliseconds(0), SimulationTick, new Milliseconds(16),
                log.SubLog("SimulationTick"));
        }

        /// <summary>
        ///     Short Lived Events are things that are short lived and forgettable in nature (less than one second of lifetime)
        ///     They can be skipped for late-joining, re-joining or re-syncing clients without any impact.
        ///     Usually used for effects like minor projectile explosions.
        /// </summary>
        public EventStream ShortLivedEventStream { get; } = new();

        public IEntityContainerWithDetectChanges AuthoritativeWorld { get; }

        public TransportStats Stats => transportWithStats.Stats;

        private void TickWorld()
        {
            Ticker.Tick(AuthoritativeWorld);
            Notifier.Notify(AuthoritativeWorld.AllEntities);
        }

        public void AssignPredictEntity(RemoteEndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            IEntity entity)
        {
            clientConnections.AssignPredictEntity(connectionId, localPlayerIndex, entity);
        }

        private void SimulationTick()
        {
            log.Debug("== Simulation Tick! {TickId}", serverTickId);

            TickWorld();
            // Exactly after Tick() and the input has been set in preparation for the next tick, we mark this as the new tick
            serverTickId = serverTickId.Next();
            log.Debug("== Simulation Tick post! {TickId}", serverTickId);
            SetInputFromClients.SetInputsFromClientsToEntities(clientConnections.Connections, serverTickId, log);

            var (masks, deltaSnapshotPack) = StoreWorldChangesToPackContainer();
            snapshotSyncer.SendSnapshot(masks, deltaSnapshotPack, AuthoritativeWorld, ShortLivedEventStream);
        }

        private (EntityMasks, DeltaSnapshotPack) StoreWorldChangesToPackContainer()
        {
            var deltaSnapshotEntityIds = Scanner.Scan(AuthoritativeWorld, serverTickId);
            var eventsThisTick = ShortLivedEventStream.FetchEventsForRange(TickIdRange.FromTickId(serverTickId));
            var deltaSnapshotPack = shouldUseBitStream
                ? DeltaSnapshotToBitPack.ToDeltaSnapshotPack(AuthoritativeWorld, eventsThisTick, deltaSnapshotEntityIds,
                    TickIdRange.FromTickId(deltaSnapshotEntityIds.TickId))
                : DeltaSnapshotToPack.ToDeltaSnapshotPack(AuthoritativeWorld, deltaSnapshotEntityIds);

            OverWriter.OverwriteAuthoritative(AuthoritativeWorld);

            return (DeltaSnapshotToEntityMasks.ToEntityMasks(deltaSnapshotEntityIds), deltaSnapshotPack);
        }


        public void Update(Milliseconds now)
        {
            clientConnections.ReceiveFromClients(serverTickId);
            simulationTicker.Update(now);
            transportWithStats.Update(now);
            log.DebugLowLevel("stats: {Stats}", transportWithStats.Stats);
        }
    }
}