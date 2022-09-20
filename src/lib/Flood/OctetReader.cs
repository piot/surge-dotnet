/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Buffers.Binary;

namespace Piot.Flood
{
    public sealed class OctetReader : IOctetReaderWithSeekAndSkip
    {
        readonly ReadOnlyMemory<byte> array;
        int pos;

        public OctetReader(ReadOnlySpan<byte> span)
        {
            array = span.ToArray();
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


        public ulong ReadUInt64()
        {
            pos += 8;
            return BinaryPrimitives.ReadUInt64BigEndian(array.Span.Slice(pos - 8, 8));
        }

        public long ReadInt64()
        {
            pos += 8;
            return BinaryPrimitives.ReadInt64BigEndian(array.Span.Slice(pos - 8, 8));
        }


        public ReadOnlySpan<byte> ReadOctets(int octetCount)
        {
            pos += octetCount;
            return array.Span.Slice(pos - octetCount, octetCount);
        }

        public void Skip(int octetCount)
        {
            pos += octetCount;
            if (pos >= array.Length)
            {
                throw new("skipped too far");
            }
        }

        public ulong Position => (ulong)pos;

        public void Seek(ulong position)
        {
            pos = (int)position;
            if (pos >= array.Length)
            {
                throw new($"seek too far {pos} vs {array.Length}");
            }
        }

        public void Dispose()
        {
            // Intentionally blank
        }
    }
}