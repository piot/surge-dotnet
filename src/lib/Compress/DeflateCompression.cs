/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;
using System.IO.Compression;

namespace Piot.Surge.Compress
{
    public static class DeflateCompression
    {
        public static ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> input)
        {
            using var outputStream = new MemoryStream();
            using (var compressor = new DeflateStream(outputStream, CompressionLevel.Fastest))
            {
                compressor.Write(input.ToArray());
            }

            return outputStream.ToArray();
        }

        public static ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> input)
        {
            var decompressedOutputStream = new MemoryStream();

            using (var inputStreamToCompress = new MemoryStream(input.ToArray()))
            {
                using (var decompressor = new DeflateStream(inputStreamToCompress, CompressionMode.Decompress))
                {
                    decompressor.CopyTo(decompressedOutputStream);
                }
            }

            return decompressedOutputStream.ToArray();
        }
    }
}