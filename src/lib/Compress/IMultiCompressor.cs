/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Compress
{
    public interface IMultiCompressor
    {
        /// <summary>
        ///     Compress the specified <paramref name="payload" />. Uses different <see cref="Compressor" /> depending on the
        ///     index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public ReadOnlySpan<byte> Compress(uint index, ReadOnlySpan<byte> payload);

        public ReadOnlySpan<byte> Decompress(uint index, ReadOnlySpan<byte> payload);
    }
}