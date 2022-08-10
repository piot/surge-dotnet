using System;
using Piot.Flood;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicInputDatagramPackOut
    {
        public static Memory<byte> CreateInputDatagram(OrderedDatagramsOut sequenceOut,
            TickId lastReceivedSnapshot, byte droppedSnapshotCount, LogicalInput.LogicalInput[] inputs)
        {
            var datagramWriter = new OctetWriter(Constants.MaxDatagramOctetSize);
            LogicInputDatagramSerialize.Serialize(datagramWriter, sequenceOut, lastReceivedSnapshot, droppedSnapshotCount, inputs);
            return datagramWriter.Octets;
        }
    }
}