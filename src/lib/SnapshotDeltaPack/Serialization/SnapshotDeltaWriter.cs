/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMaskSerialization;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    internal static class SnapshotDeltaWriter
    {
        private static void WriteEntityCount(IOctetWriter writer, int entityCount)
        {
            writer.WriteUInt16((ushort)entityCount);
        }

        /**
         * Serialized deleted, created and updated entities to the writer stream.
         */
        internal static void Write(IOctetWriter writer, EntityId[] deletedEntities,
            IEntity[] createdEntities, IUpdatedEntity[] updatedEntities)
        {
            WriteEntityCount(writer, deletedEntities.Length);
            foreach (var entityId in deletedEntities)
            {
                EntityIdWriter.Write(writer, entityId);
            }

            WriteEntityCount(writer, createdEntities.Length);
            foreach (var entity in createdEntities)
            {
                EntityIdWriter.Write(writer, entity.Id);
                writer.WriteUInt16(entity.ArchetypeId.id);
                entity.SerializeAll(writer);
            }

            WriteEntityCount(writer, updatedEntities.Length);
            foreach (var entity in updatedEntities)
            {
                EntityIdWriter.Write(writer, entity.Id);
                ChangedFieldsMaskWriter.WriteChangedFieldsMask(writer, entity.ChangeMask);
                entity.Serialize(entity.ChangeMask, writer);
            }
        }
    }
}