/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Piot.Surge.Compress
{
    public sealed class MultiCompressor : IMultiCompressor
    {
        readonly Dictionary<uint, Compressor> selection = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> Compress(uint index, ReadOnlySpan<byte> payload)
        {
            if (index == 0)
            {
                return payload;
            }

            var wasFound = selection.TryGetValue(index, out var compressor);
            if (!wasFound || compressor is null)
            {
                throw new DeserializeException($"compression index {index} not found");
            }

            return compressor.Compress(payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> Decompress(uint index, ReadOnlySpan<byte> payload)
        {
            if (index == 0)
            {
                return payload;
            }

            var wasFound = selection.TryGetValue(index, out var compressor);
            if (!wasFound || compressor is null)
            {
                throw new DeserializeException($"compression index {index} not found");
            }

            return compressor.Decompress(payload);
        }

        public void Add(uint index, Compressor compressor)
        {
            if (index == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    "compressor index 0 is reserved for no compression");
            }

            selection.Add(index, compressor);
        }
    }
}