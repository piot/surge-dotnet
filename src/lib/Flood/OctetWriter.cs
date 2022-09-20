/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Buffers.Binary;

namespace Piot.Flood
{
    public sealed class OctetWriter : IOctetWriterWithResult
    {
        readonly Memory<byte> array;

        public OctetWriter(uint size)
        {
            Position = 0;
            array = new byte[size];
        }

        public int Position { get; private set; }

        public ReadOnlySpan<byte> Octets => array.Slice(0, Position).Span;

        public void WriteUInt8(byte value)
        {
            array.Span[Position++] = value;
        }

        public void WriteInt8(sbyte value)
        {
            array.Span[Position++] = (byte)value;
        }

        public void WriteUInt16(ushort value)
        {
            Position += 2;
            BinaryPrimitives.WriteUInt16BigEndian(array.Span.Slice(Position - 2, 2), value);
        }

        public void WriteInt16(short value)
        {
            Position += 2;
            BinaryPrimitives.WriteInt16BigEndian(array.Span.Slice(Position - 2, 2), value);
        }

        public void WriteUInt32(uint value)
        {
            Position += 4;
            BinaryPrimitives.WriteUInt32BigEndian(array.Span.Slice(Position - 4, 4), value);
        }

        public void WriteInt32(int value)
        {
            Position += 4;
            BinaryPrimitives.WriteInt32BigEndian(array.Span.Slice(Position - 4, 4), value);
        }

        public void WriteUInt64(ulong value)
        {
            Position += 8;
            BinaryPrimitives.WriteUInt64BigEndian(array.Span.Slice(Position - 8, 8), value);
        }

        public void WriteInt64(long value)
        {
            Position += 8;
            BinaryPrimitives.WriteInt64BigEndian(array.Span.Slice(Position - 8, 8), value);
        }

        public void WriteOctets(ReadOnlySpan<byte> readOnlySpan)
        {
            readOnlySpan.CopyTo(array.Span.Slice(Position, readOnlySpan.Length));
            Position += readOnlySpan.Length;
        }

        public void Reset()
        {
            Position = 0;
        }
    }
}