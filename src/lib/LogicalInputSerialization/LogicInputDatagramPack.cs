using System;
using Piot.Flood;
using Piot.Surge.OrderedDatagrams;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicInputDatagramPackOut
    {
        public static Memory<byte> CreateInputDatagram(OrderedDatagramsOut sequenceOut, LogicalInput.LogicalInput[] inputs)
        {
            var datagramWriter = new OctetWriter(Constants.MaxDatagramOctetSize);
            LogicInputDatagramSerialize.Serialize(datagramWriter, sequenceOut, inputs);
            return datagramWriter.Octets;
        }
    }
}