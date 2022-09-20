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
        readonly Stack<SnapshotPack> stack = new();

        bool isInitialized;
        TickId lastInsertedTickId;

        protected int Count => stack.Count;

        protected void Push(TickId tickId, ReadOnlySpan<byte> payload)
        {
            if (isInitialized && !tickId.IsImmediateFollowing(lastInsertedTickId))
            {
                throw new($"must have stack without gaps last:{lastInsertedTickId} want to add: {tickId}");
            }

            stack.Push(new() { tickId = tickId, payload = payload.ToArray() });
            lastInsertedTickId = tickId;
            isInitialized = true;
        }

        public SnapshotPack Pop()
        {
            lastInsertedTickId = lastInsertedTickId.Previous();
            return stack.Pop();
        }

        public TickId PeekTickId()
        {
            return stack.Peek().tickId;
        }

        public void Clear()
        {
            stack.Clear();
        }
    }
}