/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.DatagramType.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.Fragment;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public static class SnapshotPackIncludingCorrectionsWriter
    {
        public delegate void SendDelegate(ReadOnlySpan<byte> datagram);

        private const uint PayloadOctetCountPerDatagram = 1100;

        public static void Write(SendDelegate send, SnapshotProtocolPack pack,
            MonotonicTimeLowerBits.MonotonicTimeLowerBits monotonicTimeLowerBits, sbyte clientInputTicksAhead,
            TickId serverTickId,
            OrderedDatagramsSequenceIdIncrease orderedDatagramsIncrease)
        {
            var datagramCount = pack.payload.Length / PayloadOctetCountPerDatagram + 1;

            var payloadSpan = pack.payload.Span;

            for (var datagramIndex = 0; datagramIndex < datagramCount; ++datagramIndex)
            {
                var fullWriter = new OctetWriter(Constants.MaxDatagramOctetSize);
                var writer = fullWriter as IOctetWriter;

                OrderedDatagramsSequenceIdWriter.Write(writer, orderedDatagramsIncrease.Value);
                DatagramTypeWriter.Write(writer, DatagramType.DatagramType.DeltaSnapshots);
                MonotonicTimeLowerBitsWriter.Write(monotonicTimeLowerBits, writer);
                writer.WriteInt8(clientInputTicksAhead);
                TickIdWriter.Write(writer, serverTickId);

                var lastOne = datagramIndex + 1 == datagramCount;

                var sliceStart = (int)(datagramIndex * PayloadOctetCountPerDatagram);
                var sliceLength = lastOne
                    ? payloadSpan.Length % (int)PayloadOctetCountPerDatagram
                    : (int)PayloadOctetCountPerDatagram;

                var payloadSlice = payloadSpan.Slice(sliceStart, sliceLength).ToArray();

                SnapshotFragmentHeaderWriter.Write(writer, pack.tickIdRange, datagramIndex, (ushort)payloadSlice.Length,
                    lastOne);
                writer.WriteOctets(payloadSlice);

                send(fullWriter.Octets.ToArray());
                orderedDatagramsIncrease.Increase();
            }
        }
    }
}