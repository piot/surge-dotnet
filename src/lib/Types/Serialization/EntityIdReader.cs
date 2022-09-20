/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class EntityIdReader
    {
        public static EntityId Read(IOctetReader reader)
        {
            return new(reader.ReadUInt16());
        }

        public static EntityId Read(IBitReader reader)
        {
            return new((ushort)reader.ReadBits(16));
        }
    }
}