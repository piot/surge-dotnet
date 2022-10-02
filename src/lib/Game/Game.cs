/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

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
        public IEntityContainerWithGhostCreator playbackWorld;
        public IMonotonicTimeMs timeProvider;
        public FixedDeltaTimeMs targetDeltaTimeMs;
        public IInputPackFetch inputFetch;
        public INotifyEntityCreation worldSync;
    }

    public sealed class Game
    {
        public Host? Host { get; }
        public Client? Client { get; }

        ILog log;

        public Game(GameInfo info, ISnapshotPlaybackNotify snapshotPlaybackNotify, ILog log)
        {
            this.log = log;
            var compressor = DefaultMultiCompressor.Create();
            var now = info.timeProvider.TimeInMs;

            var hostInfo = new HostInfo
            {
                hostTransport = info.hostTransport,
                compression = compressor,
                compressorIndex = DefaultMultiCompressor.DeflateCompressionIndex,
                authoritativeWorld = info.authoritativeWorld,
                now = now
            };

            Host = new(hostInfo, log.SubLog("Host"));

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
                ShouldApplyIncomingSnapshotsToWorld = false
            };
        }

        public void Update(TimeMs now)
        {
            Host?.Update(now);
            Client?.Update(now);
        }
    }
}