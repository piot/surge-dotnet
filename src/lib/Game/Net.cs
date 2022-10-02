/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
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
        SemanticVersion gameVersion;
        Game? game;
        readonly GameTools gameTools;
        readonly ILog log;

        public GameTools Tools => gameTools;
        public Game? Game => game;

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
            gameTools = new(gameVersion, toolsInfo, log.SubLog("GameTools"));
        }

        public (ITransport, ITransport) CreateTransports()
        {
            //var (ClientTransport, _) = MemoryTransportFactory.CreateClientAndHostTransport();
            var hostTransport = new Server(32000, log.SubLog("HostTransport"));
            var clientTransport = new Client("localhost", 32000);

            return (clientTransport, hostTransport);
        }

        GameInfo CreateGameInfo()
        {
            var (clientTransport, hostTransport) = CreateTransports();

            return new()
            {
                hostTransport = hostTransport,
                clientTransport = clientTransport,
                eventProcessor = controlInfo.eventProcessor,
                authoritativeWorld = controlInfo.authoritativeWorld,
                clientWorld = controlInfo.clientWorld,
                playbackWorld = controlInfo.playbackWorld,
                timeProvider = controlInfo.timeProvider,
                targetDeltaTimeMs = controlInfo.targetDeltaTimeMs,
                inputFetch = controlInfo.inputFetch,
                worldSync = controlInfo.worldSync
            };
        }

        public void StartHostAndClient()
        {
            if (game is not null)
            {
                throw new Exception("Game has already been started");
            }

            game = new(CreateGameInfo(), gameTools.RawSnapshotReplayRecorder, log.SubLog("Game"));
        }

        public void StartClient()
        {
            if (game is not null)
            {
                throw new Exception("Game has already been started");
            }

            game = new(CreateGameInfo(), gameTools.RawSnapshotReplayRecorder, log.SubLog("Game"));
        }

        public void Update()
        {
            var now = controlInfo.timeProvider.TimeInMs;
            gameTools.Update(now);

            game?.Update(now);
        }
    }
}