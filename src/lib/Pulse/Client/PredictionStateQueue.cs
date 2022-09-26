/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Collections;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public struct PredictionStateChecksum
    {
        public uint fnvChecksum;
        public int payloadLength;
        public ReadOnlyMemory<byte> payload;

        public bool IsEqual(ReadOnlySpan<byte> incomingPayload)
        {
            var incomingChecksum = Fnv.Fnv.ToFnv(incomingPayload);
            var checksumCompareEqual = incomingChecksum == fnvChecksum && payloadLength == incomingPayload.Length;
            var octetCompareEqual = incomingPayload.SequenceEqual(payload.Span);
            if (checksumCompareEqual != octetCompareEqual)
            {
                throw new("internal error, checksum compare and octet compare is not the same");
            }

            return octetCompareEqual;
        }
    }

    public struct PredictionStateAllChecksums
    {
        public TickId tickId;

        public PredictionStateChecksum logicChecksum;
        public PredictionStateChecksum correctionChecksum;

        public PredictionStateAllChecksums(TickId tickId, PredictionStateChecksum logicChecksum,
            PredictionStateChecksum physicsCorrection)
        {
            this.tickId = tickId;
            this.logicChecksum = logicChecksum;
            correctionChecksum = physicsCorrection;
        }

        public bool IsEqual(ReadOnlySpan<byte> logicPayload, ReadOnlySpan<byte> correctionPayload)
        {
            return logicChecksum.IsEqual(logicPayload) && correctionChecksum.IsEqual(correctionPayload);
        }
    }

    public sealed class PredictionStateChecksumQueue
    {
        readonly CircularBuffer<PredictionStateAllChecksums> queue = new(32);

        bool isInitialized;

        TickId lastInsertedTickId;

        public TickId FirstTickId => queue.Peek().tickId;
        public TickId LastTickId => queue.Last().tickId;

        public int Count => queue.Count;

        public void Enqueue(TickId tickId, ReadOnlySpan<byte> logicPayload, ReadOnlySpan<byte> physicsCorrection)
        {
            if (isInitialized && !tickId.IsImmediateFollowing(lastInsertedTickId))
            {
                throw new("must have PredictionStateChecksumQueue without gaps");
            }

            var logicFnv = Fnv.Fnv.ToFnv(logicPayload);
            var logicChecksum = new PredictionStateChecksum
                { fnvChecksum = logicFnv, payload = logicPayload.ToArray(), payloadLength = logicPayload.Length };

            var physicsCorrectionFnv = Fnv.Fnv.ToFnv(physicsCorrection);
            var physicsCorrectionChecksum = new PredictionStateChecksum
            {
                fnvChecksum = physicsCorrectionFnv, payload = physicsCorrection.ToArray(),
                payloadLength = physicsCorrection.Length
            };

            queue.Enqueue(new(tickId, logicChecksum, physicsCorrectionChecksum));
            isInitialized = true;
            lastInsertedTickId = tickId;
        }

        public PredictionStateAllChecksums? DequeueForTickId(TickId requiredTickId)
        {
            var predictionState = queue.Dequeue();
            if (predictionState.tickId.tickId != requiredTickId.tickId)
            {
                //                throw new(
                //  $"wrong internal state. prediction state dequeued is the wrong one required: {requiredTickId} encountered: {predictionState.tickId}");
                return null;
            }

            return predictionState;
        }
    }
}