/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Flood
{
    public static class BitMarker
    {
        public static void WriteMarker(IBitWriter writer, byte markValue, int bitCount = 9)
        {
            writer.WriteBits(markValue, bitCount);
        }

        public static void AssertMarker(IBitReader reader, byte expectedValue, int bitCount = 9)
        {
            var value = reader.ReadBits(bitCount);
            if (value > 0xff)
            {
                throw new($"internal marker error. got bits from other things expected {expectedValue:X2} but encountered {value:X4}");
            }

            if (value != expectedValue)
            {
                throw new($"wrong marker encountered. expected {expectedValue:X2} but encountered {value:X2}");
            }
        }
    }
}