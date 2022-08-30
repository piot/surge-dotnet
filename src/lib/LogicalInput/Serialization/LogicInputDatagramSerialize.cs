/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.DatagramType;
using Piot.Surge.LogicalInput;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotReceiveStatus;

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
            TickId lastReceivedSnapshot, byte droppedSnapshotCount, Milliseconds now,
            LogicalInputsForAllLocalPlayers inputs)
        {
            OrderedDatagramsOutWriter.Write(writer, sequenceOut);
            DatagramTypeWriter.Write(writer, DatagramType.DatagramType.PredictedInputs);
            MonotonicTimeLowerBitsWriter.Write(
                new MonotonicTimeLowerBits.MonotonicTimeLowerBits((ushort)(now.ms & 0xffff)), writer);
            SnapshotReceiveStatusWriter.Write(writer, lastReceivedSnapshot, droppedSnapshotCount);
            LogicalInputSerialize.Serialize(writer, inputs);
        }
    }
}