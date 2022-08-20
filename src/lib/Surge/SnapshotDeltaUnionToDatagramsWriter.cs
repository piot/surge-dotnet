/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.DatagramType;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public static class Constants
    {
        public const uint MaxDatagramOctetSize = 1200;
        public const uint MaxSnapshotOctetSize = 64 * MaxDatagramOctetSize;
    }

    public static class SnapshotDeltaPackUnionToDatagramsWriter
    {
        public const uint PayloadOctetCountPerDatagram = 1100;

        public static void Write(Action<Memory<byte>> send, SerializedSnapshotDeltaPackUnionFlattened pack,
            MonotonicTimeLowerBits.MonotonicTimeLowerBits monotonicTimeLowerBits, sbyte clientInputTicksAhead,
            OrderedDatagramsOutIncrease orderedDatagramsIncrease)
        {
            var datagramCount = pack.payload.Length / PayloadOctetCountPerDatagram + 1;

            var payloadSpan = pack.payload.Span;

            for (var datagramIndex = 0; datagramIndex < datagramCount; ++datagramIndex)
            {
                var writer = new OctetWriter(Constants.MaxDatagramOctetSize);

                OrderedDatagramsOutWriter.Write(writer, orderedDatagramsIncrease.Value);
                DatagramTypeWriter.Write(writer, DatagramType.DatagramType.DeltaSnapshots);
                MonotonicTimeLowerBitsWriter.Write(monotonicTimeLowerBits, writer);
                writer.WriteInt8(clientInputTicksAhead);

                var lastOne = datagramIndex + 1 == datagramCount;
                SnapshotPackDatagramHeaderWriter.Write(writer, pack.tickIdRange, datagramIndex, lastOne);

                var sliceStart = (int)(datagramIndex * PayloadOctetCountPerDatagram);
                var sliceLength = lastOne
                    ? payloadSpan.Length % (int)PayloadOctetCountPerDatagram
                    : (int)PayloadOctetCountPerDatagram;

                var payloadSlice = payloadSpan.Slice(sliceStart, sliceLength).ToArray();

                writer.WriteOctets(payloadSlice);

                send(writer.Octets);
                orderedDatagramsIncrease.Increase();
            }
        }
    }
}