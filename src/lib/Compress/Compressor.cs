/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Compress
{
    public class Compressor
    {
        public delegate ReadOnlySpan<byte> CompressDelegate(ReadOnlySpan<byte> data);

        public delegate ReadOnlySpan<byte> DecompressDelegate(ReadOnlySpan<byte> data);

        public CompressDelegate Compress;
        public DecompressDelegate Decompress;

        public Compressor(CompressDelegate compress, DecompressDelegate decompress)
        {
            Compress = compress;
            Decompress = decompress;
        }
    }
}