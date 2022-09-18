/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Piot.Collections
{
    public sealed class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        private readonly bool overwrite;
        private int head;
        private int tail;

        public CircularBuffer(int capacity, bool overwrite = true)
        {
            this.overwrite = overwrite;
            buffer = new T[capacity];
        }

        public int Capacity => buffer.Length;

        /// <summary>
        ///     Count is recommended for contiguous elements in a collection
        /// </summary>
        public int Count { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            var index = head;

            for (var i = 0; i < Count; i++, index = (index + 1) % Capacity)
            {
                yield return buffer[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            for (var i = 0; i < Capacity; i++)
            {
                buffer[i] = default!;
            }

            head = 0;
            tail = 0;
            Count = 0;
        }

        public void Enqueue(T item)
        {
            if (Count >= Capacity)
            {
                if (overwrite)
                {
                    head = (head + 1) % Capacity;
                }
                else
                {
                    throw new InvalidOperationException("Can not enqueue a full buffer");
                }
            }
            else
            {
                Count++;
            }

            buffer[tail] = item;
            tail = (tail + 1) % Capacity;
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Can not peek an empty buffer");
            }

            return buffer[head];
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Can not dequeue empty buffer");
            }

            var item = buffer[head];
            head = (head + 1) % Capacity;
            Count--;
            return item;
        }
    }
}