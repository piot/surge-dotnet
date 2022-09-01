/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Position3Writer
    {
        public static void Write(Position3 position, IOctetWriter writer)
        {
            writer.WriteInt16((short)position.x);
            writer.WriteInt16((short)position.y);
            writer.WriteInt16((short)position.z);
        }

        public static void Write(Position3 position, IBitWriter writer)
        {
            writer.WriteBits((ushort)position.x, 16);
            writer.WriteBits((ushort)position.y, 16);
            writer.WriteBits((ushort)position.z, 16);
        }
    }
}