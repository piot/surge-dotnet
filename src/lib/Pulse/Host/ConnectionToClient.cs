using System;
using Piot.Flood;
using Piot.Surge.DatagramType;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInputSerialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Surge.Transport;

namespace Piot.Surge.Pulse.Host
{
    public class ConnectionToClient
    {
        private OrderedDatagramsIn orderedDatagramsIn = new (0);
        private ClientId id;
        private LogicalInputQueue inputQueue = new();
        public ConnectionToClient(ClientId id)
        {
            this.id = id;
        }

        private void ReceivePredictedInputs(IOctetReader reader, TickId serverIsAtTickId)
        {
            var logicalInputs = LogicalInputDeserialize.Deserialize(reader);
            if (logicalInputs.Length == 0)
            {
                return;
            }

            var first = logicalInputs[0];

            if (first.appliedAtTickId > inputQueue.WaitingForTickId)
            {
                
            }

            if (first.appliedAtTickId < serverIsAtTickId)
            {
                
            }
            foreach (var logicalInput in logicalInputs)
            {
                inputQueue.AddLogicalInput(logicalInput);
            }
            
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