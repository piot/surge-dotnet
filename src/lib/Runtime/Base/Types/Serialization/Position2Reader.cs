/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Position2Reader
    {
        public static void Read(IOctetReader reader, ref Position2 position)
        {
            position.x = reader.ReadInt32();
            position.y = reader.ReadInt32();
        }
    }
}