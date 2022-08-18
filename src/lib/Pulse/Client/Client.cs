/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.DatagramType;
using Piot.Surge.LogicalInput;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotSerialization;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    public class Client
    {
        private readonly ILog log;
        private readonly ITransportClient transport;
        private readonly ClientPredictor predictor;
        private readonly ClientDeltaSnapshotPlayback deltaSnapshotPlayback;
        private ClientWorld world;
        private OrderedDatagramsIn orderedDatagramsIn = new (0);
        
        public Client(ILog log, Milliseconds now, Milliseconds targetDeltaTimeMs, IEntityCreation entityCreation, ITransportClient transport, IInputPackFetch fetch)
        {
            this.log = log;
            world = new ClientWorld(entityCreation);
            this.transport = transport;
            predictor = new ClientPredictor(fetch, now, targetDeltaTimeMs, log.SubLog("Predictor"));
            deltaSnapshotPlayback = new ClientDeltaSnapshotPlayback(now, (world as IEntityContainerWithCreation), predictor, targetDeltaTimeMs, log.SubLog("GhostPlayback"));
        }

        private void ReceiveSnapshot(IOctetReader reader)
        {
            var unionOfSnapshots = SnapshotDeltaUnionReader.Read(reader);
            deltaSnapshotPlayback.FeedSnapshotsUnion(unionOfSnapshots);
        }
        
        private void ReceiveDatagramFromHost(IOctetReader reader)
        {
            var sequenceIn = OrderedDatagramsInReader.Read(reader);
            if (orderedDatagramsIn.IsValidSuccessor(sequenceIn))
            {
                orderedDatagramsIn = new OrderedDatagramsIn(sequenceIn.Value);
            }
            else
            {
                log.DebugLowLevel("ordered datagram in wrong order, discarding datagram");
                return;
            }

            var datagramType = DatagramTypeReader.Read(reader);
            switch (datagramType)
            {
                case DatagramType.DatagramType.DeltaSnapshots:
                    ReceiveSnapshot(reader);
                    break;
                default:
                    throw new Exception($"illegal datagram type {datagramType} from host");
            }
        }
        
        private void ReceiveDatagramsFromHost()
        {
            for (var i = 0; i < 30; i++)
            {
                var datagram = transport.ReceiveFromHost();
                if (datagram.IsEmpty)
                {
                    return;
                }

                var datagramReader = new OctetReader(datagram.ToArray());
                ReceiveDatagramFromHost(datagramReader);
            }
        }
        
        public void Update(Milliseconds now)
        {
            ReceiveDatagramsFromHost();
            predictor.Update(now);
            deltaSnapshotPlayback.Update(now);
        }
    }
}