/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.Pulse.Client
{
    public struct TickIdPack
    {
        public ReadOnlyMemory<byte> payload;
        public TickId tickId;
    }

    public class TickIdPackQueue
    {
        private readonly Queue<TickIdPack> queue = new();

        public TickId FirstTickId => queue.Peek().tickId;

        public int Count => queue.Count;

        protected void Enqueue(TickId tickId, ReadOnlySpan<byte> payload)
        {
            queue.Enqueue(new() { tickId = tickId, payload = payload.ToArray() });
        }

        public TickIdPack Dequeue()
        {
            return queue.Dequeue();
        }
    }
}