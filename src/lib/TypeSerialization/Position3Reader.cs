/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Types;

namespace Piot.Surge.TypeSerialization
{
    public static class Position3Reader
    {
        public static Position3 Read(IOctetReader reader)
        {
            return new()
            {
                x = reader.ReadInt16(),
                y = reader.ReadInt16(),
                z = reader.ReadInt16()
            };
        }
    }
}