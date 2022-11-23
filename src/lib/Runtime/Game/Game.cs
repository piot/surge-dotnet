/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge;
using Piot.Surge.Compress;
using Piot.Surge.Core;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Pulse.Host;
using Piot.Transport;

namespace Surge.Game
{
    public struct GameInfo
    {
        public ITransport? hostTransport;
        public ITransport clientTransport;
        public IEventReceiver eventProcessor;
        public IEntityContainerWithDetectChanges hostDetectChanges;
        public IDataSender hostDataSender;

        public IDataReceiver clientDataReceiver;
        public IMonotonicTimeMs timeProvider;
        public FixedDeltaTimeMs targetDeltaTimeMs;
    }

    public enum GameMode
    {
        HostAndClient,
        ClientOnly
    }

    public class NullEntityManagerReceiver : IEntityManagerReceiver
    {

        public void ReceiveMultipleComponentsFullFiltered(IBitReader bitReader, uint entityId, uint[] dataTypeIds)
        {
        }
    }


    public sealed class Game
    {
        ILog log;

        public Game(GameInfo info, ClientDeltaSnapshotPlayback.SnapshotPlaybackDelegate? snapshotPlaybackNotify,
            Action<ConnectionToClient>? onCreatedConnection, GameMode mode, ILog log)
        {
            this.log = log;
            var compressor = DefaultMultiCompressor.Create();
            var now = info.timeProvider.TimeInMs;

            if (mode == GameMode.HostAndClient)
            {

                var hostInfo = new HostInfo
                {
                    hostTransport = info.hostTransport!,
                    compression = compressor,
                    compressorIndex = DefaultMultiCompressor.NoCompressionIndex,
                    detectChanges = info.hostDetectChanges,
                    now = now,
                    onConnectionCreated = onCreatedConnection!,
                    targetDeltaTimeMs = info.targetDeltaTimeMs,
                    dataSender = info.hostDataSender,
                    entityManagerReceiver = new NullEntityManagerReceiver()
                };

                Host = new(hostInfo, log.SubLog("Host"));
            }

            var clientInfo = new ClientInfo
            {
                now = info.timeProvider.TimeInMs,
                targetDeltaTimeMs = info.targetDeltaTimeMs,
                worldWithGhostCreator = info.clientDataReceiver,
                componentsWriter = info.hostDataSender,
                eventProcessor = info.eventProcessor,
                assignedTransport = info.clientTransport,
                compression = compressor,
                snapshotPlaybackNotify = snapshotPlaybackNotify!
            };

            Client = new(clientInfo, log.SubLog("Client"))
            {
                ShouldApplyIncomingSnapshotsToWorld = mode == GameMode.ClientOnly, UsePrediction = mode == GameMode.ClientOnly
            };
        }

        public Host? Host { get; }
        public Client? Client { get; }

        public void PreTick()
        {
            Host?.PreTick();
        }
        public void Update(TimeMs now)
        {
            Host?.Update(now);
            Client?.Update(now);
        }

        public void PostTick()
        {
            Host?.PostTick();
        }
    }
}