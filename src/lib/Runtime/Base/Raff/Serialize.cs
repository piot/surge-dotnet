/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Raff
{
    public static class Serialize
    {
        /// <summary>
        ///     Write a header according to the <a href="https://github.com/piot/raff-c#readme">RAFF</a> standard.
        ///     Must be the first thing written to the stream or file.
        /// </summary>
        /// <param name="writer"></param>
        public static void WriteHeader(IOctetWriter writer)
        {
            writer.WriteOctets(Constants.fileHeader);
        }

        public static void WriteChunk(IOctetWriter writer, FourCC icon, FourCC name, ReadOnlySpan<byte> octets)
        {
            writer.WriteUInt32(icon.Value);
            writer.WriteUInt32(name.Value);
            writer.WriteUInt32((uint)octets.Length);
            writer.WriteOctets(octets);
        }

        // WriteIntraChunkMarker writes a octet slice to file with an extended header.
        public static void WriteIntraChunkMarker(IOctetWriter writer, FourCC icon)
        {
            writer.WriteUInt32(icon.Value);
        }
    }
}