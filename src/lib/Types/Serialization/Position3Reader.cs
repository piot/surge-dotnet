/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
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

        public static Position3 Read(IBitReader reader)
        {
            return new()
            {
                x = (int)reader.ReadBits(16),
                y = (int)reader.ReadBits(16),
                z = (int)reader.ReadBits(16)
            };
        }
    }
}