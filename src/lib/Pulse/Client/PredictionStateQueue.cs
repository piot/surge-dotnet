/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public struct PredictionStateChecksum
    {
        public uint fnvChecksum;
        public TickId tickId;
        public int payloadLength;
        public ReadOnlyMemory<byte> payload;

        public bool IsEqual(ReadOnlySpan<byte> incomingPayload)
        {
            var incomingChecksum = Fnv.Fnv.ToFnv(incomingPayload);
            var checksumCompareEqual = incomingChecksum == fnvChecksum && payloadLength == incomingPayload.Length;
            var octetCompareEqual = incomingPayload.SequenceEqual(payload.Span);
            if (checksumCompareEqual != octetCompareEqual)
            {
                throw new Exception("internal error, checksum compare and octet compare is not the same");
            }

            return octetCompareEqual;
        }
    }

    public class PredictionStateChecksumQueue
    {
        private readonly Queue<PredictionStateChecksum> queue = new();

        public TickId FirstTickId => queue.Peek().tickId;

        public int Count => queue.Count;

        public void Enqueue(TickId tickId, ReadOnlySpan<byte> payload)
        {
            var fnv = Fnv.Fnv.ToFnv(payload);
            queue.Enqueue(new()
                { tickId = tickId, fnvChecksum = fnv, payload = payload.ToArray(), payloadLength = payload.Length });
        }

        public PredictionStateChecksum DequeueForTickId(TickId requiredTickId)
        {
            var predictionState = queue.Dequeue();
            if (predictionState.tickId.tickId != requiredTickId.tickId)
            {
                throw new Exception(
                    $"wrong internal state. prediction state dequeued is the wrong one required: {requiredTickId} encountered: {predictionState.tickId}");
            }

            return predictionState;
        }
    }
}