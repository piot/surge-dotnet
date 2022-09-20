/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Surge.Tick;

namespace Piot.Surge.Replay.Serialization
{
    public sealed class CompleteState
    {
        readonly ReadOnlyMemory<byte> payload;

        public CompleteState(TimeMs capturedAtTimeMs, TickId tickId, ReadOnlySpan<byte> payload)
        {
            CapturedAtTimeMs = capturedAtTimeMs;
            TickId = tickId;
            this.payload = payload.ToArray();
        }

        public ReadOnlySpan<byte> Payload => payload.Span;
        public TickId TickId { get; }

        public TimeMs CapturedAtTimeMs { get; }

        public override string ToString()
        {
            return $"[CompleteState {CapturedAtTimeMs}   {TickId} octetCount: {payload.Length}]";
        }
    }
}