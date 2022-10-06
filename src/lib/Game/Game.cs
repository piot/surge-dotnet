/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge;
using Piot.Surge.Compress;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Pulse.Host;
using Piot.Transport;

namespace Surge.Game
{
    public struct GameInfo
    {
        public ITransport hostTransport;
        public ITransport clientTransport;
        public IEventProcessor eventProcessor;
        public IEntityContainerWithDetectChanges authoritativeWorld;

        public IEntityContainerWithGhostCreator clientWorld;

        //public IEntityContainerWithGhostCreator playbackWorld;
        public IMonotonicTimeMs timeProvider;
        public FixedDeltaTimeMs targetDeltaTimeMs;
        public IInputPackFetch inputFetch;
        public INotifyEntityCreation worldSync;
    }

    public enum GameMode
    {
        HostAndClient,
        ClientOnly
    }

    public sealed class Game
    {
        ILog log;

        public Game(GameInfo info, ISnapshotPlaybackNotify snapshotPlaybackNotify,
            Action<ConnectionToClient>? onCreatedConnection, GameMode mode, ILog log)
        {
            this.log = log;
            var compressor = DefaultMultiCompressor.Create();
            var now = info.timeProvider.TimeInMs;

            if (mode == GameMode.HostAndClient)
            {
                var hostInfo = new HostInfo
                {
                    hostTransport = info.hostTransport,
                    compression = compressor,
                    compressorIndex = DefaultMultiCompressor.DeflateCompressionIndex,
                    authoritativeWorld = info.authoritativeWorld,
                    now = now,
                    onConnectionCreated = onCreatedConnection!,
                    targetDeltaTimeMs = info.targetDeltaTimeMs
                };

                Host = new(hostInfo, log.SubLog("Host"));
            }

            var clientInfo = new ClientInfo
            {
                now = info.timeProvider.TimeInMs,
                targetDeltaTimeMs = info.targetDeltaTimeMs,
                worldWithGhostCreator = info.clientWorld,
                eventProcessor = info.eventProcessor,
                assignedTransport = info.clientTransport,
                compression = compressor,
                fetch = info.inputFetch,
                snapshotPlaybackNotify = snapshotPlaybackNotify
            };

            Client = new(clientInfo, log.SubLog("Client"))
            {
                ShouldApplyIncomingSnapshotsToWorld = mode == GameMode.ClientOnly,
                UsePrediction = mode == GameMode.ClientOnly
            };
        }

        public Host? Host { get; }
        public Client? Client { get; }

        public void Update(TimeMs now)
        {
            Host?.Update(now);
            Client?.Update(now);
        }
    }
}