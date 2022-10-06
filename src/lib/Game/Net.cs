/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

#nullable enable

using System;
using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.SerializableVersion;
using Piot.Surge;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.Pulse.Host;
using Piot.Transport;
using Piot.UdpServer;

namespace Surge.Game
{
    public struct ControlInfo
    {
        public IEventProcessor eventProcessor;
        public IEntityContainerWithDetectChanges authoritativeWorld;
        public IEntityContainerWithGhostCreator clientWorld;
        public IEntityContainerWithGhostCreator playbackWorld;
        public IMonotonicTimeMs timeProvider;
        public FixedDeltaTimeMs targetDeltaTimeMs;
        public IInputPackFetch inputFetch;
        public INotifyEntityCreation worldSync;
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
                clientWorld = controlInfo.clientWorld,
                playbackWorld = controlInfo.playbackWorld,
                worldSync = controlInfo.worldSync,
                eventProcessor = controlInfo.eventProcessor,
                timeProvider = controlInfo.timeProvider
            };
            Tools = new(gameVersion, toolsInfo, log.SubLog("GameTools"));
        }

        public GameTools Tools { get; }

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
            const int minHalfLatency = 350 / 2;
            const int maxHalfLatency = 450 / 2;

            clientHazyTransport.In.LatencySimulator.SetLatencyRange(minHalfLatency, maxHalfLatency);
            clientHazyTransport.Out.LatencySimulator.SetLatencyRange(minHalfLatency, maxHalfLatency);

            clientHazyTransport.In.Decision.SetChances(0.00002d, 0, 0.01d, 0.001d);
            clientHazyTransport.Out.Decision.SetChances(0.00002d, 0, 0.01d, 0.001d);

            return clientTransport;
        }

       
        GameInfo CreateGameInfo(bool createHostTransport)
        {
            ITransport hostTransport = null;
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
                authoritativeWorld = controlInfo.authoritativeWorld,
                clientWorld = controlInfo.clientWorld,
                timeProvider = controlInfo.timeProvider,
                targetDeltaTimeMs = controlInfo.targetDeltaTimeMs,
                inputFetch = controlInfo.inputFetch,
                worldSync = controlInfo.worldSync
            };
        }

        public void StartHostAndClient(Action<ConnectionToClient> onCreatedConnection)
        {
            if (Game is not null)
            {
                throw new("Game has already been started");
            }

            Game = new(CreateGameInfo(true), Tools.RawSnapshotReplayRecorder, onCreatedConnection, GameMode.HostAndClient,
                log.SubLog("Game"));
        }

        public void StartClient()
        {
            if (Game is not null)
            {
                throw new("Game has already been started");
            }

            var gameInfo = CreateGameInfo(false);
            gameInfo.clientWorld = controlInfo.playbackWorld;

            Game = new(gameInfo, Tools.RawSnapshotReplayRecorder, null, GameMode.ClientOnly,
                log.SubLog("Game"));
        }

        public void Update()
        {
            var now = controlInfo.timeProvider.TimeInMs;
            Tools.Update(now);
            hazyClientTransport?.Update();
            Game?.Update(now);
        }
    }
}