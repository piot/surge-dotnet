/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public struct SnapshotPack
    {
        public ReadOnlyMemory<byte> payload;
        public TickId tickId;
    }

    public class SnapshotPackQueue
    {
        private readonly Queue<SnapshotPack> queue = new();

        protected void Enqueue(TickId tickId, ReadOnlySpan<byte> payload)
        {
            queue.Enqueue(new() { tickId = tickId, payload = payload.ToArray() });
        }

        public SnapshotPack Dequeue()
        {
            return queue.Dequeue();
        }
    }
}