using Piot.Flood;
using Piot.Surge.DatagramType;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicInputDatagramSerialize
    {
        /// <summary>
        ///     Serialize logical input according to
        ///     https://github.com/piot/surge-dotnet/blob/main/doc/protocol.adoc#predicted-logical-input-datagrams
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="inputs"></param>
        public static void Serialize(IOctetWriter writer, OrderedDatagramsOut sequenceOut,
            TickId lastReceivedSnapshot, byte droppedSnapshotCount, LogicalInput.LogicalInput[] inputs)
        {
            OrderedDatagramsOutWriter.Write(writer, sequenceOut);
            DatagramTypeWriter.Write(writer, DatagramType.DatagramType.PredictedInputs);
            SnapshotReceiveStatusWriter.Write(writer, lastReceivedSnapshot, droppedSnapshotCount);
            LogicalInputSerialize.Serialize(writer, inputs);
        }
    }
}