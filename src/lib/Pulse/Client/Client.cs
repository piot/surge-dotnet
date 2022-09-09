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
        private readonly ILog log;
        private readonly ClientPredictor predictor;
        private readonly ITransportClient transportClient;
        private readonly TransportStatsBoth transportWithStats;

        public Client(ILog log, Milliseconds now, Milliseconds targetDeltaTimeMs,
            IEntityContainerWithGhostCreator worldWithGhostCreator, IEventProcessorWithCreate eventProcessorWithCreate,
            ITransport assignedTransport, IMultiCompressor compression, IInputPackFetch fetch)
        {
            this.log = log;

            World = worldWithGhostCreator;
            transportWithStats = new(assignedTransport, now);
            transportClient = new TransportClient(transportWithStats);
            predictor = new ClientPredictor(fetch, transportClient, now, targetDeltaTimeMs,
                worldWithGhostCreator,
                log.SubLog("Predictor"));
            deltaSnapshotPlayback =
                new ClientDeltaSnapshotPlayback(now, worldWithGhostCreator, eventProcessorWithCreate, predictor,
                    targetDeltaTimeMs,
                    log.SubLog("GhostPlayback"));

            datagramReceiver = new(transportClient, compression, deltaSnapshotPlayback, predictor, log);
        }

        public IInputPackFetch InputFetch
        {
            set => predictor.InputFetch = value;
        }

        public IEntityContainerWithGhostCreator World { get; }

        public void Update(Milliseconds now)
        {
            datagramReceiver.ReceiveDatagramsFromHost(now);
            transportWithStats.Update(now);
            predictor.Update(now);
            deltaSnapshotPlayback.Update(now);
            var readStats = transportWithStats.Stats;
            log.DebugLowLevel("stats: {Stats}", readStats);
            log.DebugLowLevel("netStats: {Stats}", datagramReceiver.NetworkQuality);
        }
    }
}