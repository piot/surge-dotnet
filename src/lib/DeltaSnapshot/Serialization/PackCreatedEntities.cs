/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class PackCreatedEntity
    {
        /// <summary>
        ///     Writes the EntityID, ArchetypeID and all the fields in the Entity.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="entityId"></param>
        /// <param name="archetypeId"></param>
        /// <param name="entitySerializer"></param>
        /// <exception cref="Exception"></exception>
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