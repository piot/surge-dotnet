/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class EntityIdWriter
    {
        public static void Write(IOctetWriter writer, EntityId id)
        {
            writer.WriteUInt16(id.Value);
        }

        public static void Write(IBitWriter writer, EntityId id)
        {
            writer.WriteBits(id.Value, 16);
        }
    }
}