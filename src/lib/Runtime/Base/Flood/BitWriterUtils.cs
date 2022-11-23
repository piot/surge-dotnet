/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Flood
{
    public static class BitWriterUtils
    {
        public static void WriteSignedBits(IBitWriter writer, int signedValue, int bits)
        {
            var moved = signedValue + (1 << bits - 1);
            if (moved < 0)
            {
                throw new($"value {signedValue} was moved to {moved} bitcount:{bits}");
            }

            writer.WriteBits((uint)moved, bits);
        }
    }
}