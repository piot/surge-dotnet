/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class PackCreatedEntity
    {
        public static void Write(IOctetWriter writer, EntityId entityId, ArchetypeId archetypeId,
            IEntitySerializer entitySerializer)
        {
            EntityIdWriter.Write(writer, entityId);
            if (archetypeId.id == 0)
            {
                throw new Exception("illegal archetype id");
            }

            writer.WriteUInt16(archetypeId.id);
            entitySerializer.SerializeAll(writer);
        }
    }
}