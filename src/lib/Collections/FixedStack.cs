/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;

namespace Piot.Collections
{
    public sealed class FixedStack<T> : IEnumerable<T>
    {
        readonly CircularBuffer<T> entries;

        public FixedStack(int capacity)
        {
            entries = new(capacity, false);
        }

        public int Count => entries.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            entries.Clear();
        }

        public void Push(T data)
        {
            entries.Enqueue(data);
        }

        public T Pop()
        {
            return entries.PopTail();
        }

        public T PeekBottom()
        {
            return entries.PeekTail();
        }

        public void RemoveBottom()
        {
            entries.Remove();
        }

        public T Peek()
        {
            return entries.PeekTail();
        }
    }
}