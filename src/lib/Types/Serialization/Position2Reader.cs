/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Position2Reader
    {
        public static Position2 Read(IOctetReader reader)
        {
            return new()
            {
                x = reader.ReadInt32(),
                y = reader.ReadInt32()
            };
        }
    }
}