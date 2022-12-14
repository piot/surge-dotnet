/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

#nullable enable

using System;
using Piot.Surge.Ecs2;
using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.SerializableVersion;
using Piot.Surge;
using Piot.Surge.Core;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Pulse.Host;
using Piot.Surge.Tick;
using Piot.Transport;
using Piot.UdpServer;

namespace Surge.Game
{
    public interface IEntityContainerToWorld
    {
    }

    public struct ControlInfo
    {
        public IEventReceiver eventProcessor;
        public IMonotonicTimeMs timeProvider;
        public FixedDeltaTimeMs targetDeltaTimeMs;
        public IDataSender hostDataSender;
        public IDataReceiver hostDataReceiver;
        public IDataReceiver clientDataReceiver;
        public IEntityContainerWithDetectChanges hostDetectChanges;
    }

    public sealed class Net
    {
        readonly ControlInfo controlInfo;
        readonly ILog log;
        SemanticVersion gameVersion;
        IUpdateTransport? hazyClientTransport;

        public Net(SemanticVersion gameVersion, ControlInfo controlInfo, ILog log)
        {
            this.log = log;
            this.controlInfo = controlInfo;
            this.gameVersion = gameVersion;

            var toolsInfo = new GameToolsInfo
            {
                eventProcessor = controlInfo.eventProcessor, timeProvider = controlInfo.timeProvider
            };
            
            log.Debug("Surge Net is started");
            
            // HACK!!!! TODO:
            //Tools = new(gameVersion, toolsInfo, log.SubLog("GameTools"));
        }

        public GameTools? Tools { get; }

        public Game? Game { get; private set; }

        public ITransport CreateHostTransport()
        {
            return new Server(32000, log.SubLog("HostTransport"));
        }

        public ITransport CreateClientTransport()
        {
            var clientTransport = new Client("localhost", 32000, log.SubLog("ClientTransport"));
            var clientHazyTransport = new InternetSimulatorTransport(clientTransport, controlInfo.timeProvider,
                new PseudoRandom(97), log.SubLog("Hazy"));
            const int minHalfLatency = 0 / 2;
            const int maxHalfLatency = 10 / 2;

            clientHazyTransport.In.LatencySimulator.SetLatencyRange(minHalfLatency, maxHalfLatency);
            clientHazyTransport.Out.LatencySimulator.SetLatencyRange(minHalfLatency, maxHalfLatency);

            clientHazyTransport.In.Decision.SetChances(0.00002d, 0, 0.01d, 0.001d);
            clientHazyTransport.Out.Decision.SetChances(0.00002d, 0, 0.01d, 0.001d);

            hazyClientTransport = clientHazyTransport;
            return clientHazyTransport;
        }


        GameInfo CreateGameInfo(bool createHostTransport)
        {
            ITransport? hostTransport = null;
            if (createHostTransport)
            {
                hostTransport = CreateHostTransport();
            }

            var clientTransport = CreateClientTransport();

            return new()
            {
                hostTransport = hostTransport,
                clientTransport = clientTransport,
                eventProcessor = controlInfo.eventProcessor,
                timeProvider = controlInfo.timeProvider,
                targetDeltaTimeMs = controlInfo.targetDeltaTimeMs,
                hostDataSender = controlInfo.hostDataSender,
                hostDataReceiver = controlInfo.hostDataReceiver,
                hostDetectChanges = controlInfo.hostDetectChanges,
                clientDataReceiver = controlInfo.clientDataReceiver
            };
        }

        public void StartHostAndClient(Action<ConnectionToClient> onCreatedConnection, Action hostSimulationTickRunSystems)
        {
            if (Game is not null)
            {
                throw new("Game has already been started");
            }

            Game = new(CreateGameInfo(true), Tools is null ? OnSnapshotPlayback : Tools.RawSnapshotReplayRecorder.SnapshotPlaybackNotify, onCreatedConnection, hostSimulationTickRunSystems,
                GameMode.HostAndClient,
                log.SubLog("Game"));
        }

        void OnSnapshotPlayback(TimeMs timeNowMs, TickId tickIdNow, DeltaSnapshotPack deltaSnapshotPack)
        {

        }

        public void StartClient()
        {
            if (Game is not null)
            {
                throw new("Game has already been started");
            }

            var gameInfo = CreateGameInfo(false);

            Game = new(gameInfo, Tools is null ? null : Tools.RawSnapshotReplayRecorder.SnapshotPlaybackNotify, null, null, GameMode.ClientOnly,
                log.SubLog("Game"));
        }


        public void PreTick()
        {
            Game?.PreTick();
            Game?.Tick();
        }

        public void Update()
        {
            var now = controlInfo.timeProvider.TimeInMs;
            Tools?.Update(now);
            hazyClientTransport?.Update();
            Game?.Update(now);
        }

        public void PostTick()
        {
            Game?.PostTick();
        }
    }
}