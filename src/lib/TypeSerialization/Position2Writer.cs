/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Types;

namespace Piot.Surge.TypeSerialization
{
    public static class Position2Writer
    {
        public static void Write(Position2 position, IOctetWriter writer)
        {
            writer.WriteInt16((short)position.x);
            writer.WriteInt16((short)position.y);
        }
    }
}