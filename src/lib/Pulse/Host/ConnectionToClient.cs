/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.DatagramType;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInputSerialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotReceiveStatus;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public class ConnectionToClient
    {
        private readonly RemoteEndpointId id;
        private readonly LogicalInputQueue inputQueue = new();
        private readonly ILog log;
        private readonly SnapshotSyncerClient syncer;
        private OrderedDatagramsIn orderedDatagramsIn = new(0);

        public ConnectionToClient(RemoteEndpointId id, SnapshotSyncerClient syncer, ILog log)
        {
            this.id = id;
            this.log = log;
            this.syncer = syncer;
        }

        private void ReceivePredictedInputs(IOctetReader reader, TickId serverIsAtTickId)
        {
            log.DebugLowLevel("received predicted inputs");
            syncer.lastReceivedMonotonicTimeLowerBits = MonotonicTimeLowerBitsReader.Read(reader);
            SnapshotReceiveStatusReader.Read(reader, out var tickId, out var droppedFrames);

            var logicalInputs = LogicalInputDeserialize.Deserialize(reader);
            if (logicalInputs.Length == 0)
            {
                log.Notice("it is strange the predicted inputs does not contain any values {ServerTickId}",
                    serverIsAtTickId);
                return;
            }

            var first = logicalInputs[0];

            if (first.appliedAtTickId.tickId > inputQueue.WaitingForTickId.tickId)
            {
                return;
            }

            if (first.appliedAtTickId.tickId < serverIsAtTickId.tickId)
            {
                return;
            }

            foreach (var logicalInput in logicalInputs)
            {
                inputQueue.AddLogicalInput(logicalInput);
            }

            syncer.clientInputTickCountAheadOfServer = (sbyte)inputQueue.Collection.Length;
        }

        public void Receive(IOctetReader reader, TickId serverIsAtTickId)
        {
            var sequenceIn = OrderedDatagramsInReader.Read(reader);
            if (orderedDatagramsIn.IsValidSuccessor(sequenceIn))
            {
                orderedDatagramsIn = new OrderedDatagramsIn(sequenceIn.Value);
            }

            var datagramType = DatagramTypeReader.Read(reader);
            switch (datagramType)
            {
                case DatagramType.DatagramType.PredictedInputs:
                    ReceivePredictedInputs(reader, serverIsAtTickId);
                    break;
                default:
                    throw new Exception($"illegal datagram type {datagramType} from client ${id}");
            }
        }
    }
}