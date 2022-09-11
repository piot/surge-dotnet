/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Flood
{
    public interface IBitWriter
    {
        public void WriteBits(uint bits, int bitCount);

        public void CopyBits(IBitReader reader, int bitCount);
    }
}