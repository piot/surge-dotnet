/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Surge.Tick;

namespace Piot.Surge.Replay.Serialization
{
    public sealed class DeltaState
    {
        readonly ReadOnlyMemory<byte> payload;

        public DeltaState(TimeMs timeProcessedMs, TickIdRange tickId, ReadOnlySpan<byte> payload)
        {
            TimeProcessedMs = timeProcessedMs;
            TickIdRange = tickId;
            this.payload = payload.ToArray();
        }

        public ReadOnlySpan<byte> Payload => payload.Span;
        public TickIdRange TickIdRange { get; }
        public TimeMs TimeProcessedMs { get; }

        public override string ToString()
        {
            return $"[DeltaState {TimeProcessedMs} {TickIdRange} octetCount: {payload.Length}]";
        }
    }
}