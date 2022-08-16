/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicInputDatagramPackOut
    {
        /// <summary>
        ///     Creates an Input Datagram that is ready to send over the transport.
        ///     Calls <see cref="LogicInputDatagramSerialize.Serialize" />.
        /// </summary>
        /// <param name="sequenceOut"></param>
        /// <param name="lastReceivedSnapshot"></param>
        /// <param name="droppedSnapshotCount"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static Memory<byte> CreateInputDatagram(OrderedDatagramsOut sequenceOut,
            TickId lastReceivedSnapshot, byte droppedSnapshotCount, LogicalInput.LogicalInput[] inputs)
        {
            var datagramWriter = new OctetWriter(Constants.MaxDatagramOctetSize);
            LogicInputDatagramSerialize.Serialize(datagramWriter, sequenceOut, lastReceivedSnapshot,
                droppedSnapshotCount, inputs);
            return datagramWriter.Octets;
        }
    }
}