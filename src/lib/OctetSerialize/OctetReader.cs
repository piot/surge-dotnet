/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Buffers.Binary;

namespace Piot.Surge.OctetSerialize
{
    public class OctetReader : IOctetReader
    {
        private readonly ReadOnlyMemory<byte> array;
        private int pos;

        public OctetReader(Memory<byte> memory)
        {
            array = memory;
        }

        public byte ReadUInt8()
        {
            return array.Span[pos++];
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

        public ulong ReadUInt64()
        {
            pos += 8;
            return BinaryPrimitives.ReadUInt64BigEndian(array.Span.Slice(pos - 8, 8));
        }

        public byte[] ReadOctets(int octetCount)
        {
            pos += octetCount;
            return array.Span.Slice(pos - octetCount, octetCount).ToArray();
        }
    }
}