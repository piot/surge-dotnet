/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Fnv
{
    public static class Fnv
    {
        private const uint Prime = 16777619u;
        private const uint Initial = 2166136261u;

        public static uint ToFnv(ReadOnlySpan<byte> payload)
        {
            var hash = Initial;

            foreach (var t in payload)
            {
                hash ^= t;
                hash *= Prime;
            }

            return hash;
        }
    }
}