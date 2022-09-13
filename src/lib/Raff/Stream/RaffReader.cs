/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Raff.Stream
{
    public sealed class RaffReader
    {
        private readonly IOctetReader reader;

        public RaffReader(IOctetReader reader)
        {
            this.reader = reader;
            DeSerialize.ReadHeader(reader);
        }

        public uint ReadChunkHeader(out FourCC icon, out FourCC name)
        {
            return DeSerialize.ReadChunkHeader(reader, out icon, out name);
        }

        public ChunkHeader ReadChunkHeader()
        {
            return DeSerialize.ReadChunkHeader(reader);
        }

        public uint ReadExpectedChunkHeader(FourCC icon, FourCC name)
        {
            return DeSerialize.ReadExpectedChunkHeader(reader, icon, name);
        }

        public ReadOnlySpan<byte> ReadChunk(out FourCC icon, out FourCC name)
        {
            return DeSerialize.ReadChunk(reader, out icon, out name);
        }

        public ReadOnlySpan<byte> ReadExpectedChunk(FourCC icon, FourCC name)
        {
            return DeSerialize.ReadExpectedChunk(reader, icon, name);
        }
    }
}