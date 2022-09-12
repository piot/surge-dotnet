/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.Event;
using Piot.Surge.LogicalInput;
using Piot.Transport;
using Piot.Transport.Stats;

namespace Piot.Surge.Pulse.Client
{
    public class Client
    {
        private readonly ClientDatagramReceiver datagramReceiver;
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private readonly ClientLocalInputFetch localInputFetch;
        private readonly ILog log;
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;

        public Client(ILog log, Milliseconds now, Milliseconds targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator, IEventProcessor eventProcessorWithCreate,
            ITransport assignedTransport, IMultiCompressor compression, IInputPackFetch fetch)
        {
            this.log = log;

            World = worldWithGhostCreator;
            transportWithStats = new(assignedTransport, now);
            transportClient = new TransportClient(transportWithStats);
            var clientPredictor = new ClientPredictor(log.SubLog("ClientPredictor"));
            const bool usePrediction = false;
            localInputFetch = new ClientLocalInputFetch(fetch, clientPredictor, usePrediction, transportClient, now,
                targetDeltaTimeMs,
                worldWithGhostCreator,
                log.SubLog("Predictor"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, eventProcessorWithCreate, localInputFetch,
                    targetDeltaTimeMs,
                    log.SubLog("GhostPlayback"));

            datagramReceiver = new(transportClient, compression, deltaSnapshotPlayback, localInputFetch, log);
        }

        public bool ShouldPlayIncomingSnapshots { get; set; } = true;

        public IInputPackFetch InputFetch
        {
            set => localInputFetch.InputFetch = value;
        }

        public LocalPlayersInfo LocalPlayersInfo => new(localInputFetch.LocalPlayerInputs);

        public IEntityContainerWithGhostCreator World { get; }

        public void Update(Milliseconds now)
        {
            datagramReceiver.ReceiveDatagramsFromHost(now);
            transportWithStats.Update(now);
            localInputFetch.Update(now);
            if (ShouldPlayIncomingSnapshots)
            {
                deltaSnapshotPlayback.Update(now);
            }
            else
            {
                deltaSnapshotPlayback.ClearSnapshots();
            }

            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", datagramReceiver.NetworkQuality);
        }
    }
}