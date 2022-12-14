/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.Ecs2;
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
        public IEntityContainerWithDetectChanges detectChanges;
        public TimeMs now;
        public Action<ConnectionToClient> onConnectionCreated;
        public FixedDeltaTimeMs targetDeltaTimeMs;
        public IDataSender dataSender;
        public IEntityManagerReceiver entityManagerReceiver;
        public Action simulationTickRunSystems;
    }

    public sealed class Host
    {
        readonly BitWriter cachedSnapshotBitWriter = new(Constants.MaxSnapshotOctetSize);
        readonly ClientConnections clientConnections;
        readonly IDataSender dataSender;
        readonly IEntityManagerReceiver entityManagerReceiver;
        readonly ILog log;
        readonly ILog inputLog;
        readonly SnapshotSyncer snapshotSyncer;
        readonly TimeTicker statsTicker;
        readonly ITransport transport;
        readonly TransportStatsBoth transportWithStats;
        TickId authoritativeTickId;
        readonly Action simulationTickRunSystems;

        public Host(HostInfo info, ILog log)
        {
            inputLog = log.SubLog("Input");
            simulationTickRunSystems = info.simulationTickRunSystems;
            transportWithStats = new(info.hostTransport, info.now);
            transport = transportWithStats;
            snapshotSyncer = new(transport, info.compression, info.compressorIndex, log.SubLog("SnapshotSyncer"));
            AuthoritativeWorld = info.detectChanges;
            dataSender = info.dataSender;
            clientConnections = new(info.hostTransport, snapshotSyncer, info.onConnectionCreated, log);
            entityManagerReceiver = info.entityManagerReceiver;
            this.log = log;
            /*
            simulationTicker = new(info.now, SimulationTick, info.targetDeltaTimeMs,
                log.SubLog("SimulationTick"));
                */
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

        public void AssignEntityToControl(EndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            EntityId entity, bool shouldPredict)
        {
            clientConnections.AssignEntityToControl(connectionId, localPlayerIndex, entity, shouldPredict);
        }

        public void AssignPlayerSlotEntity(EndpointId connectionId, LocalPlayerIndex localPlayerIndex,
            EntityId entity)
        {
            clientConnections.AssignPlayerSlotEntity(connectionId, localPlayerIndex, entity);
        }

        void StatsOutput()
        {
            log.DebugLowLevel("stats: {Stats}", transportWithStats.Stats);
        }

        public void ResetTime(TimeMs now)
        {
            statsTicker.Reset(now);
        }

        public void PreTick()
        {
            clientConnections.ReceiveFromClients(authoritativeTickId);
            authoritativeTickId = authoritativeTickId.Next;
            ShortLivedEventStream.EndOfTick(authoritativeTickId);
            //log.Debug("== Simulation Tick post! {TickId}", authoritativeTickId);

            SetInputFromClients.SetInputsFromClientsToEntities(clientConnections.Connections, authoritativeTickId, entityManagerReceiver, inputLog);
        }

        public void Tick()
        {
            simulationTickRunSystems();
        }

        public void PostTick()
        {
            StoreChangesMadeToTheWorld();
            cachedSnapshotBitWriter.Reset();
            snapshotSyncer.SendSnapshotToClients(dataSender, ShortLivedEventStream, authoritativeTickId);
        }

        void StoreChangesMadeToTheWorld()
        {
            var changes = AuthoritativeWorld.EntitiesThatHasChanged(log);
            changes.TickId = authoritativeTickId;
            snapshotSyncer.History.Enqueue(changes);
        }

        public void Update(TimeMs now)
        {
            transportWithStats.Update(now);
            statsTicker.Update(now);
        }
    }
}