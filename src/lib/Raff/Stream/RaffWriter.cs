/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Raff.Stream
{
    public class RaffWriter
    {
        private readonly IOctetWriter writer;

        public RaffWriter(IOctetWriter writer)
        {
            this.writer = writer;
            Serialize.WriteHeader(writer);
        }

        public void WriteChunk(FourCC icon, FourCC name, ReadOnlySpan<byte> octets)
        {
            Serialize.WriteChunk(writer, icon, name, octets);
        }

        public void Close()
        {
            Serialize.WriteChunk(writer, new(0), new(0), ReadOnlySpan<byte>.Empty);
        }
    }
}