/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Compress
{
    public struct CompressorIndex
    {
        public uint Index { get; }

        public CompressorIndex(uint index)
        {
            Index = index;
        }
    }
}