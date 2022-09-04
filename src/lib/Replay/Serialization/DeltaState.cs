/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Replay.Serialization
{
    public class DeltaState
    {
        private readonly ReadOnlyMemory<byte> payload;

        public DeltaState(TickIdRange tickId, ReadOnlySpan<byte> payload)
        {
            TickIdRange = tickId;
            this.payload = payload.ToArray();
        }

        public ReadOnlySpan<byte> Payload => payload.Span;
        public TickIdRange TickIdRange { get; }
    }
}