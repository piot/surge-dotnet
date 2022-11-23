/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Flood
{
    public interface IOctetReader
    {
        public byte ReadUInt8();
        public sbyte ReadInt8();
        public ushort ReadUInt16();
        public short ReadInt16();
        public uint ReadUInt32();
        public int ReadInt32();

        public ulong ReadUInt64();
        public long ReadInt64();

        public ReadOnlySpan<byte> ReadOctets(int octetCount);
    }
}