/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Types;

namespace Piot.Surge.TypeSerialization
{
    public static class Position3Writer
    {
        public static void Write(Position3 position, IOctetWriter writer)
        {
            writer.WriteInt16((short)position.x);
            writer.WriteInt16((short)position.y);
            writer.WriteInt16((short)position.z);
        }
    }
}