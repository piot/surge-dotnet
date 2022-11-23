/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Position2Writer
    {
        public static void Write(IOctetWriter writer, Position2 position)
        {
            writer.WriteInt32((short)position.x);
            writer.WriteInt32((short)position.y);
        }
    }
}