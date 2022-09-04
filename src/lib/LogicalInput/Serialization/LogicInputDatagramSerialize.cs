/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.DatagramType.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.ReceiveStatus;
using Piot.Surge.Tick;

namespace Piot.Surge.LogicalInput.Serialization
{
    public static class LogicInputDatagramSerialize
    {
        /// <summary>
        ///     Serialize logical input according to
        ///     https://github.com/piot/surge-dotnet/blob/main/doc/protocol.adoc#predicted-logical-input-datagrams
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="inputs"></param>
        public static void Serialize(IOctetWriter writer, OrderedDatagramsSequenceId sequenceOut,
            TickId lastReceivedSnapshot, byte droppedSnapshotCount, Milliseconds now,
            LogicalInputsForAllLocalPlayers inputs)
        {
            OrderedDatagramsSequenceIdWriter.Write(writer, sequenceOut);
            DatagramTypeWriter.Write(writer, DatagramType.DatagramType.PredictedInputs);
            MonotonicTimeLowerBitsWriter.Write(
                new MonotonicTimeLowerBits.MonotonicTimeLowerBits((ushort)(now.ms & 0xffff)), writer);
            SnapshotReceiveStatusWriter.Write(writer, lastReceivedSnapshot, droppedSnapshotCount);
            LogicalInputSerialize.Serialize(writer, inputs);
        }
    }
}