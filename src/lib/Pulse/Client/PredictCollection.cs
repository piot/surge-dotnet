/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Piot.Collections;
using Piot.Maths;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public struct PredictItem
    {
        public TickId tickId;
        public ReadOnlyMemory<byte> undoPack;
        public ReadOnlyMemory<byte> logicStatePack;
        public ReadOnlyMemory<byte> physicsStatePack;
        public ReadOnlyMemory<byte> inputPackSetBeforeThisTick;
        public uint logicStateFnvChecksum;
        public uint physicsStateFnvChecksum;
    }

    public class PredictCollection : IEnumerable<PredictItem>
    {
        public readonly CircularBuffer<PredictItem> items = new(64, false);
        public int indexInCircularBuffer = -1;

        public PredictItem[] Items => items.ToArray();

        public TickId FirstTickId => items.Peek().tickId;
        public TickId LastTickId => items.PeekTail().tickId;

        public TickId TickId => Count == 0
            ? throw new("can not read tick id from empty predict collection")
            : items.GetAt(indexInCircularBuffer).tickId;

        public int Count => items.Count;

        public PredictItem Current => items.GetAt(indexInCircularBuffer);

        public IEnumerator<PredictItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public PredictItem[] RemainingItems()
        {
            var list = new List<PredictItem>();
            for (var i = indexInCircularBuffer + 1; i < Count; ++i)
            {
                list.Add(items[i]);
            }

            return list.ToArray();
        }

        public void EnqueuePredict(TickId tickId, ReadOnlySpan<byte> undoPack, ReadOnlySpan<byte> inputPack,
            ReadOnlySpan<byte> logicStatePack, ReadOnlySpan<byte> physicsStatePack)
        {
            if (logicStatePack.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(logicStatePack));
            }

            if (undoPack.Length == 0)
            {
                //throw new ArgumentOutOfRangeException(nameof(undoPack));
            }

            var before = indexInCircularBuffer;
            indexInCircularBuffer++;
            indexInCircularBuffer = BaseMath.Modulus(indexInCircularBuffer, items.Capacity);

            if (tickId.tickId == 0)
            {
                throw new("Suspicious");
            }

            if (items.Count > 0)
            {
                if (items[before].tickId != tickId.Previous)
                {
                    throw new($"Must have increasing previous {items[before].tickId} but expected {tickId.Previous}");
                }
            }

            items.SetAndAdvance(indexInCircularBuffer, new()
            {
                tickId = tickId,
                undoPack = undoPack.ToArray(),
                logicStatePack = logicStatePack.ToArray(),
                physicsStatePack = physicsStatePack.ToArray(),
                inputPackSetBeforeThisTick = inputPack.ToArray(),
                logicStateFnvChecksum = Fnv.Fnv.ToFnv(logicStatePack),
                physicsStateFnvChecksum = Fnv.Fnv.ToFnv(physicsStatePack)
            });
        }

        public void Reset()
        {
            items.Clear();
        }

        public PredictItem PopRollback()
        {
            return items.PopTail();
        }

        public PredictItem GoRollback()
        {
            var v = Current;
            if (indexInCircularBuffer > 0)
            {
                MovePrevious();
            }

            return v;
        }

        public void MovePrevious()
        {
            if (indexInCircularBuffer <= 0)
            {
                throw new InvalidOperationException("can not go back");
            }

            indexInCircularBuffer--;
        }

        public void MoveNext()
        {
            if (indexInCircularBuffer + 1 >= Count)
            {
                throw new InvalidOperationException("can not go forward");
            }

            indexInCircularBuffer++;
        }

        public PredictItem? FindFromTickId(TickId tickId)
        {
            foreach (var predictItem in items)
            {
                if (predictItem.tickId.tickId == tickId.tickId)
                {
                    return predictItem;
                }
            }

            return null;
        }

        public void DiscardUpToAndExcluding(TickId targetTickId)
        {
            while (items.Count > 0)
            {
                if (items.Peek().tickId.tickId < targetTickId.tickId)
                {
                    if (indexInCircularBuffer > 0)
                    {
                        indexInCircularBuffer--;
                    }

                    items.Remove();
                }
                else
                {
                    break;
                }
            }
        }
    }
}