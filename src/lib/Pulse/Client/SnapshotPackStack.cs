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
    public struct SnapshotPack
    {
        public ReadOnlyMemory<byte> payload;
        public TickId tickId;
    }

    public class SnapshotPackStack
    {
        bool isInitialized;
        TickId lastInsertedTickId;
        protected FixedStack<SnapshotPack> stack = new(64);

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

        public SnapshotPack GetPackFromTickId(TickId tickId)
        {
            foreach (var pack in stack)
            {
                if (pack.tickId == tickId)
                {
                    return pack;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(tickId), "is not in stack");
        }

        public void DiscardUpToAndExcluding(TickId correctionForTickId)
        {
            while (stack.Count > 0)
            {
                if (stack.PeekBottom().tickId >= correctionForTickId)
                {
                    return;
                }

                stack.RemoveBottom();
            }
        }

        public TickId PeekTickId()
        {
            return stack.Peek().tickId;
        }

        public TickId EndTickId()
        {
            return stack.Last().tickId;
        }

        public void Clear()
        {
            stack.Clear();
        }
    }
}