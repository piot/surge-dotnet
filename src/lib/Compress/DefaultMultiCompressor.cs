/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Compress
{
    public static class DefaultMultiCompressor
    {
        public static readonly CompressorIndex NoCompressionIndex = new(0);
        public static readonly CompressorIndex DeflateCompressionIndex = new(1);

        public static IMultiCompressor Create()
        {
            var multiCompressor = new MultiCompressor();

            multiCompressor.Add(DeflateCompressionIndex.Index, DeflateCompressorCreator.Create());

            return multiCompressor;
        }
    }
}