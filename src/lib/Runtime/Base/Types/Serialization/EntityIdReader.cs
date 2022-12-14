/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class EntityIdReader
    {
        public static void Read(IBitReader reader, out EntityId id)
        {
            BitMarker.AssertMarker(reader, 0xaf);
            id.Value = (ushort)reader.ReadBits(16);
        }
    }

    public static class ComponentTypeIdReader
    {
        public static ComponentTypeId Read(IBitReader reader)
        {
            return new((ushort)reader.ReadBits(8));
        }
    }
}