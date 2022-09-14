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
        private readonly Memory<byte> array;
        private int pos;

        public OctetWriter(uint size)
        {
            pos = 0;
            array = new byte[size];
        }

        public ReadOnlySpan<byte> Octets => array.Slice(0, pos).Span;

        public void WriteUInt8(byte value)
        {
            array.Span[pos++] = value;
        }

        public void WriteInt8(sbyte value)
        {
            array.Span[pos++] = (byte)value;
        }

        public void WriteUInt16(ushort value)
        {
            pos += 2;
            BinaryPrimitives.WriteUInt16BigEndian(array.Span.Slice(pos - 2, 2), value);
        }

        public void WriteInt16(short value)
        {
            pos += 2;
            BinaryPrimitives.WriteInt16BigEndian(array.Span.Slice(pos - 2, 2), value);
        }

        public void WriteUInt32(uint value)
        {
            pos += 4;
            BinaryPrimitives.WriteUInt32BigEndian(array.Span.Slice(pos - 4, 4), value);
        }

        public void WriteInt32(int value)
        {
            pos += 4;
            BinaryPrimitives.WriteInt32BigEndian(array.Span.Slice(pos - 4, 4), value);
        }

        public void WriteUInt64(ulong value)
        {
            pos += 8;
            BinaryPrimitives.WriteUInt64BigEndian(array.Span.Slice(pos - 8, 8), value);
        }

        public void WriteInt64(long value)
        {
            pos += 8;
            BinaryPrimitives.WriteInt64BigEndian(array.Span.Slice(pos - 8, 8), value);
        }

        public void WriteOctets(ReadOnlySpan<byte> readOnlySpan)
        {
            readOnlySpan.CopyTo(array.Span.Slice(pos, readOnlySpan.Length));
            pos += readOnlySpan.Length;
        }

        public void Reset()
        {
            pos = 0;
        }
    }
}