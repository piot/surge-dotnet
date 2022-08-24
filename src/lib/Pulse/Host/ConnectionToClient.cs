/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Net.NetworkInformation;
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
        private OrderedDatagramsIn orderedDatagramsIn;
        public EntityId ControllingEntityId;

        public LogicalInputQueue InputQueue => inputQueue;
        public RemoteEndpointId Id => id;

        public bool HasAssignedEntity => false;

        
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

            if (first.appliedAtTickId.tickId > inputQueue.WaitingForTickId.tickId && inputQueue.IsInitialized)
            {
                log.Notice(
                    $"there is a gap in the input queue. Input queue is waiting for {inputQueue.WaitingForTickId} but first received in this datagram {first.appliedAtTickId}");
                inputQueue.Reset();
            }


            foreach (var logicalInput in logicalInputs)
            {
                if (logicalInput.appliedAtTickId.tickId < serverIsAtTickId.tickId)
                {
                    continue;
                }

                if (logicalInput.appliedAtTickId.tickId > inputQueue.WaitingForTickId.tickId)
                {
                    //
                }
                inputQueue.AddLogicalInput(logicalInput);
            }

            log.DebugLowLevel("input Queue on server for connection {ConnectionId} is {Count}", Id, inputQueue.Count);
            syncer.clientInputTickCountAheadOfServer = (sbyte)inputQueue.Count;
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