/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.Compress;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Client
{
    public sealed class Client
    {
        private readonly ClientDatagramReceiver datagramReceiver;
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private readonly ClientLocalInputFetch localInputFetch;
        private readonly ILog log;
        private readonly ClientSnapshotRecorder snapshotRecorder;
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;

        public Client(ILog log, TimeMs now, FixedDeltaTimeMs targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator, IEventProcessor eventProcessor,
            ITransport assignedTransport, IMultiCompressor compression, IInputPackFetch fetch,
            SemanticVersion applicationVersion)
        {
            this.log = log;

            World = worldWithGhostCreator;
            snapshotRecorder =
                new ClientSnapshotRecorder(applicationVersion, worldWithGhostCreator, eventProcessor,
                    log.SubLog("SnapshotRecorder"));

            transportWithStats = new(assignedTransport, now);
            transportClient = new TransportClient(transportWithStats);
            var clientPredictor = new ClientPredictor(log.SubLog("ClientPredictor"));
            const bool usePrediction = false;
            localInputFetch = new ClientLocalInputFetch(fetch, clientPredictor, usePrediction, transportClient, now,
                targetDeltaTimeMs,
                worldWithGhostCreator,
                log.SubLog("Predictor"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, eventProcessor, localInputFetch,
                    snapshotRecorder.OnSnapshotPlayback, targetDeltaTimeMs,
                    log.SubLog("GhostPlayback"));

            datagramReceiver = new(transportClient, compression, deltaSnapshotPlayback, localInputFetch, log);
        }

        public bool ShouldApplyIncomingSnapshotsToWorld
        {
            get => deltaSnapshotPlayback.ShouldApplySnapshotsToWorld;
            set => deltaSnapshotPlayback.ShouldApplySnapshotsToWorld = value;
        }

        public IReplayControl ReplayControl => snapshotRecorder;

        public TickId PlaybackTickId => deltaSnapshotPlayback.PlaybackTickId;

        public IInputPackFetch InputFetch
        {
            set => localInputFetch.InputFetch = value;
        }

        public LocalPlayersInfo LocalPlayersInfo => new(localInputFetch.LocalPlayerInputs);

        public IEntityContainerWithGhostCreator World { get; }

        public void Update(TimeMs now)
        {
            datagramReceiver.ReceiveDatagramsFromHost(now);
            transportWithStats.Update(now);
            localInputFetch.Update(now);
            deltaSnapshotPlayback.Update(now);
            snapshotRecorder.Update(now);

            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", datagramReceiver.NetworkQuality);
        }
    }
}