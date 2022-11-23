/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;

namespace Piot.Flood
{
    public sealed class StreamOctetReader : IOctetReaderWithSeekAndSkip
    {
        readonly Stream stream;

        public StreamOctetReader(Stream stream)
        {
            this.stream = stream;
        }

        public byte ReadUInt8()
        {
            var buffer = ReadOctets(1);
            return buffer[0];
        }

        public sbyte ReadInt8()
        {
            return (sbyte)ReadUInt8();
        }

        public ushort ReadUInt16()
        {
            var buffer = ReadOctets(2);
            return (ushort)(buffer[0] << 8 | buffer[1]);
        }

        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public uint ReadUInt32()
        {
            var buffer = ReadOctets(4);
            return (uint)buffer[0] << 24 | (uint)buffer[1] << 16 | (uint)buffer[2] << 8 | buffer[3];
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            var buffer = ReadOctets(8);
            return (ulong)buffer[0] << 56 | (ulong)buffer[1] << 48 | (ulong)buffer[2] << 40 |
                   (ulong)buffer[3] << 32 | (ulong)buffer[4] << 24 |
                   (ulong)buffer[5] << 16 | (ulong)buffer[6] << 8 | buffer[7];
        }

        public long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public ReadOnlySpan<byte> ReadOctets(int octetCount)
        {
            var temporaryBuffer = new byte[octetCount];
            var octetsRead = stream.Read(temporaryBuffer);
            if (octetsRead != octetCount)
            {
                throw new EndOfStreamException();
            }

            return temporaryBuffer;
        }

        public void Skip(int octetCount)
        {
            stream.Seek(octetCount, SeekOrigin.Current);
        }

        public void Seek(ulong position)
        {
            stream.Seek((long)position, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public ulong Position => (ulong)stream.Position;
    }
}