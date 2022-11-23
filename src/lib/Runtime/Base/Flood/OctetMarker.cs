/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Flood
{
    public static class OctetMarker
    {
        public static void WriteMarker(IOctetWriter writer, byte markValue)
        {
            writer.WriteUInt8(markValue);
        }

        public static void AssertMarker(IOctetReader reader, byte expectedValue)
        {
            var value = reader.ReadUInt8();
            if (value != expectedValue)
            {
                throw new("wrong marker encountered");
            }
        }
    }
}