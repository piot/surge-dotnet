/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
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

namespace Piot.Surge.Pulse.Host
{
    public class Host
    {
        private readonly ClientConnections clientConnections;
        private readonly ILog log;
        private readonly TimeTicker simulationTicker;
        private readonly SnapshotSyncer snapshotSyncer;
        private readonly ITransport transport;
        private readonly TransportStatsBoth transportWithStats;
        private TickId authoritativeTickId;

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
            log.Debug("== Simulation Tick! {TickId}", authoritativeTickId);

            TickWorld();
            // Exactly after Tick() and the input has been set in preparation for the next tick, we mark this as the new tick
            authoritativeTickId = authoritativeTickId.Next();
            ShortLivedEventStream.EndOfTick(authoritativeTickId);
            log.Debug("== Simulation Tick post! {TickId}", authoritativeTickId);

            SetInputFromClients.SetInputsFromClientsToEntities(clientConnections.Connections, authoritativeTickId, log);

            var (masks, deltaSnapshotPack) = StoreWorldChanges.StoreWorldChangesToPackContainer(AuthoritativeWorld,
                ShortLivedEventStream, authoritativeTickId, SnapshotStreamType.BitStream);
            snapshotSyncer.SendSnapshot(masks, deltaSnapshotPack, AuthoritativeWorld, ShortLivedEventStream);
        }

        public void Update(Milliseconds now)
        {
            clientConnections.ReceiveFromClients(authoritativeTickId);
            simulationTicker.Update(now);
            transportWithStats.Update(now);
            log.DebugLowLevel("stats: {Stats}", transportWithStats.Stats);
        }
    }
}