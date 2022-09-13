/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Replay.Serialization
{
    public sealed class CompleteState
    {
        private readonly ReadOnlyMemory<byte> payload;

        public CompleteState(TickId tickId, ReadOnlySpan<byte> payload)
        {
            TickId = tickId;
            this.payload = payload.ToArray();
        }

        public ReadOnlySpan<byte> Payload => payload.Span;
        public TickId TickId { get; }

        public override string ToString()
        {
            return $"[CompleteState {TickId} octetCount: {payload.Length}]";
        }
    }
}