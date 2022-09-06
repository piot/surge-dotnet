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

    public class SnapshotPackStack
    {
        private readonly Stack<SnapshotPack> queue = new();

        private bool isInitialized;
        private TickId lastInsertedTickId;

        protected int Count => queue.Count;

        protected void Push(TickId tickId, ReadOnlySpan<byte> payload)
        {
            if (isInitialized && !tickId.IsImmediateFollowing(lastInsertedTickId))
            {
                throw new Exception("must have queue without gaps");
            }

            queue.Push(new() { tickId = tickId, payload = payload.ToArray() });
            lastInsertedTickId = tickId;
            isInitialized = true;
        }

        public SnapshotPack Pop()
        {
            return queue.Pop();
        }
    }
}