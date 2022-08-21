/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Buffers.Binary;

namespace Piot.Flood
{
    public class OctetReader : IOctetReader
    {
        private readonly ReadOnlyMemory<byte> array;
        private int pos;

        public OctetReader(ReadOnlyMemory<byte> memory)
        {
            array = memory;
        }

        public byte ReadUInt8()
        {
            return array.Span[pos++];
        }

        public sbyte ReadInt8()
        {
            return (sbyte)array.Span[pos++];
        }

        public ushort ReadUInt16()
        {
            pos += 2;
            return BinaryPrimitives.ReadUInt16BigEndian(array.Span.Slice(pos - 2, 2));
        }

        public short ReadInt16()
        {
            pos += 2;
            return BinaryPrimitives.ReadInt16BigEndian(array.Span.Slice(pos - 2, 2));
        }

        public uint ReadUInt32()
        {
            pos += 4;
            return BinaryPrimitives.ReadUInt32BigEndian(array.Span.Slice(pos - 4, 4));
        }

        public int ReadInt32()
        {
            pos += 4;
            return BinaryPrimitives.ReadInt32BigEndian(array.Span.Slice(pos - 4, 4));
        }


        public ReadOnlySpan<byte> ReadOctets(int octetCount)
        {
            pos += octetCount;
            return array.Span.Slice(pos - octetCount, octetCount);
        }
    }
}