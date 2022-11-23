/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;

namespace Piot.Flood
{
    public sealed class StreamOctetWriter : IDisposableOctetWriter
    {
        readonly Stream stream;

        public StreamOctetWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void WriteUInt8(byte value)
        {
            WriteOctets(new[]
            {
                value
            });
        }

        public void WriteInt8(sbyte value)
        {
            WriteUInt8((byte)value);
        }

        public void WriteUInt16(ushort value)
        {
            WriteOctets(new[]
            {
                (byte)(value >> 8), (byte)value
            });
        }

        public void WriteInt16(short value)
        {
            WriteUInt16((ushort)value);
        }

        public void WriteUInt32(uint value)
        {
            WriteOctets(new[]
            {
                (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value
            });
        }

        public void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }

        public void WriteUInt64(ulong value)
        {
            WriteOctets(new[]
            {
                (byte)(value >> 56), (byte)(value >> 48), (byte)(value >> 40), (byte)(value >> 32), (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value
            });
        }

        public void WriteInt64(long value)
        {
            WriteUInt64((ulong)value);
        }

        public void WriteOctets(ReadOnlySpan<byte> value)
        {
            stream.Write(value);
        }

        void IDisposable.Dispose()
        {
            stream.Dispose();
        }
    }
}