/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class UnitVector2Writer
    {
        public static void Write(UnitVector2 position, IOctetWriter writer)
        {
            writer.WriteInt32((short)position.x);
            writer.WriteInt32((short)position.y);
        }

        public static void Write(UnitVector2 position, IBitWriter writer)
        {
            BitWriterUtils.WriteSignedBits(writer, position.x, 20);
            BitWriterUtils.WriteSignedBits(writer, position.y, 20);
        }
    }
}