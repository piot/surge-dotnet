/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Raff.Stream
{
    public sealed class RaffWriter
    {
        readonly IOctetWriter writer;
        bool isClosed;

        public RaffWriter(IOctetWriter writer)
        {
            this.writer = writer;
            Serialize.WriteHeader(writer);
        }

        public void WriteChunk(FourCC icon, FourCC name, ReadOnlySpan<byte> octets)
        {
            if (isClosed)
            {
                throw new("can not write, has already been closed");
            }

            Serialize.WriteChunk(writer, icon, name, octets);
        }

        public void Close()
        {
            if (isClosed)
            {
                throw new("has already been closed");
            }

            Serialize.WriteChunk(writer, new(0), new(0), ReadOnlySpan<byte>.Empty);
            isClosed = true;
        }
    }
}