/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Compress
{
    public static class DeflateCompressorCreator
    {
        public static Compressor Create()
        {
            return new(DeflateCompression.Compress, DeflateCompression.Decompress);
        }
    }
}