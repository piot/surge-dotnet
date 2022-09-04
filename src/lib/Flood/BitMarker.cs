/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Flood
{
    public static class BitMarker
    {
        public static void WriteMarker(IBitWriter writer, byte markValue)
        {
            writer.WriteBits(markValue, 9);
        }

        public static void AssertMarker(IBitReader reader, byte expectedValue)
        {
            var value = reader.ReadBits(9);
            if (value != expectedValue)
            {
                throw new Exception("wrong marker encountered");
            }
        }
    }
}