/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Client
{
    public sealed class Client
    {
        private readonly ClientDatagramReceiver datagramReceiver;
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private readonly ClientLocalInputFetchAndSend localInputFetchAndSend;
        private readonly ILog log;
        private readonly TimeTicker statsTicker;
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;

        public Client(ILog log, TimeMs now, FixedDeltaTimeMs targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator, IEventProcessor eventProcessor,
            ITransport assignedTransport, IMultiCompressor compression, IInputPackFetch fetch,
            ISnapshotPlaybackNotify snapshotPlaybackNotify)
        {
            this.log = log;

            World = worldWithGhostCreator;


            transportWithStats = new(assignedTransport, now);
            transportClient = new TransportClient(transportWithStats);
            var clientPredictor = new ClientPredictor(log.SubLog("ClientPredictor"));
            const bool usePrediction = false;
            localInputFetchAndSend = new ClientLocalInputFetchAndSend(fetch, clientPredictor, usePrediction,
                transportClient, now,
                targetDeltaTimeMs,
                worldWithGhostCreator,
                log.SubLog("InputFetchAndSend"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, eventProcessor, localInputFetchAndSend,
                    snapshotPlaybackNotify, targetDeltaTimeMs,
                    log.SubLog("SnapshotPlayback"));

            datagramReceiver = new(transportClient, compression, deltaSnapshotPlayback, localInputFetchAndSend, log);
            statsTicker = new(new(0), StatsOutput, new(1000), log.SubLog("Stats"));
        }

        public bool ShouldApplyIncomingSnapshotsToWorld
        {
            get => deltaSnapshotPlayback.ShouldTickAndNotifySnapshots;
            set => deltaSnapshotPlayback.ShouldTickAndNotifySnapshots = value;
        }

        public TickId PlaybackTickId => deltaSnapshotPlayback.PlaybackTickId;

        public IInputPackFetch InputFetch
        {
            set => localInputFetchAndSend.InputFetch = value;
        }

        public LocalPlayersInfo LocalPlayersInfo => new(localInputFetchAndSend.LocalPlayerInputs);

        public IEntityContainerWithGhostCreator World { get; }

        private void StatsOutput()
        {
            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", datagramReceiver.NetworkQuality);
        }

        public void Update(TimeMs now)
        {
            datagramReceiver.ReceiveDatagramsFromHost(now);
            transportWithStats.Update(now);
            localInputFetchAndSend.Update(now);
            deltaSnapshotPlayback.Update(now);
            statsTicker.Update(now);
        }
    }
}