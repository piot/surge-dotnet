/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

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
            if (value != expectedValue)
            {
                throw new Exception("wrong marker encountered");
            }
        }
    }
}