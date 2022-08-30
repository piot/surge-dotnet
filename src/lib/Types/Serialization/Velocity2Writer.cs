/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Types;

namespace Piot.Surge.Types.Serialization
{
    public static class Velocity2Writer
    {
        public static void Write(Velocity2 position, IOctetWriter writer)
        {
            writer.WriteInt16((short)position.x);
            writer.WriteInt16((short)position.y);
        }
    }
}