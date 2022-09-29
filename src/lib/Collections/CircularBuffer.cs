/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using Piot.Maths;

namespace Piot.Collections
{
    public sealed class CircularBuffer<T> : IEnumerable<T>
    {
        readonly T[] buffer;
        readonly bool overwrite;
        internal int head;
        internal int tail;

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


        public T this[int key]
        {
            get => GetAt(key);
            set => SetAt(key, value);
        }

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

        public T GetAt(int index)
        {
            if (index < 0 || index >= Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var internalIndex = (head + index) % Capacity;
            return buffer[internalIndex];
        }

        public void SetAt(int index, T item)
        {
            if (index < 0 || index >= Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var internalIndex = (head + index) % Capacity;
            buffer[internalIndex] = item;
        }


        public void SetAndAdvance(int index, T item)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SetAt(index, item);

            var internalIndex = (head + index) % Capacity;
            if (internalIndex == tail)
            {
                Count++;
                tail = (tail + 1) % Capacity;
            }
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

        public void RemoveTail()
        {
            if (Count == 0)
            {
                throw new("can not remove head, buffer is empty");
            }

            tail = BaseMath.Modulus(tail - 1, Capacity);
            Count--;
        }

        public T PopTail()
        {
            if (Count == 0)
            {
                throw new("can not remove head, buffer is empty");
            }

            tail = BaseMath.Modulus(tail - 1, Capacity);
            Count--;

            return buffer[tail];
        }

        public void Remove()
        {
            if (Count == 0)
            {
                throw new("can not remove head, buffer is empty");
            }

            head = (head + 1) % Capacity;
            Count--;
        }


        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Can not peek an empty buffer");
            }

            return buffer[head];
        }

        public T PeekTail()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Can not peek an empty buffer");
            }

            var index = BaseMath.Modulus(tail - 1, Capacity);
            return buffer[index];
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