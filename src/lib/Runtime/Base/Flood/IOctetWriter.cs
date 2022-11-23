/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Flood
{
    public interface IOctetWriter
    {
        public void WriteUInt8(byte value);
        public void WriteInt8(sbyte value);
        public void WriteUInt16(ushort value);
        public void WriteInt16(short value);
        public void WriteUInt32(uint value);
        public void WriteInt32(int value);
        public void WriteUInt64(ulong value);
        public void WriteInt64(long value);
        public void WriteOctets(ReadOnlySpan<byte> value);
    }
}