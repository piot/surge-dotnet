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
        private ClientGhostPlayback ghostPlayback;
        private ClientWorld world;
        private OrderedDatagramsIn orderedDatagramsIn = new (0);
        
        public Client(ILog log, Milliseconds now, IEntityCreation entityCreation, ITransportClient transport, IInputPackFetch fetch)
        {
            this.log = log;
            world = new ClientWorld(entityCreation);
            this.transport = transport;
            predictor = new ClientPredictor(fetch, now, log.SubLog("Predictor"));
            ghostPlayback = new ClientGhostPlayback(now, world, predictor, log.SubLog("GhostPlayback"));
        }

        private void ReceiveSnapshot(IOctetReader reader)
        {
            var unionOfSnapshots = SnapshotDeltaUnionReader.Read(reader);
            ghostPlayback.FeedSnapshotsUnion(unionOfSnapshots);
        }
        
        private void ReceiveDatagramFromHost(IOctetReader reader)
        {
            var sequenceIn = OrderedDatagramsInReader.Read(reader);
            if (orderedDatagramsIn.IsValidSuccessor(sequenceIn))
            {
                orderedDatagramsIn = new OrderedDatagramsIn(sequenceIn.Value);
            }

            var datagramType = DatagramTypeReader.Read(reader);
            switch (datagramType)
            {
                case DatagramType.DatagramType.DeltaSnapshots:
                    ReceiveSnapshot(reader);
                    break;
                default:
                    throw new Exception($"illegal datagram type {datagramType} from client ${id}");
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
            ghostPlayback.Update(now);
        }
    }
}