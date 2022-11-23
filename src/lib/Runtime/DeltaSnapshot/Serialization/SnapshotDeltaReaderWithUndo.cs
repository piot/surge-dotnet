/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public sealed class SnapshotDeltaReaderInfoEntity
    {
        public readonly ulong changeMask;
        public readonly EntityId entity;

        public SnapshotDeltaReaderInfoEntity(EntityId entity, ulong changeMask)
        {
            this.entity = entity;
            this.changeMask = changeMask;
        }
    }

    /*
    public static class SnapshotDeltaReaderWithUndo
    {
        /// <summary>
        ///     Similar to <see cref="SnapshotDeltaReader.Read" />, but it also writes the current values of the fields that are
        ///     overwritten.
        ///     The undoWriter can be used to undo the overwritten entities.
        /// </summary>
        /// <param name="reader">Octet stream to read from.</param>
        /// <param name="entityGhostContainer">Interface that handles the entityGhostContainer of new entities.</param>
        /// <param name="undoWriter">Octet stream to write the undo stream to.</param>
        /// <returns></returns>
        public static (EntityId[], EntityId[], SnapshotDeltaReaderInfoEntity[]) ReadWithUndo(IOctetReader reader,
            IDataReceiver entityGhostContainer,
            IOctetWriter undoWriter)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaSync)
            {
                throw new("out of sync");
            }

            undoWriter.WriteUInt8(Constants.SnapshotDeltaSync);
#endif
            var deletedEntities = new List<EntityId>();
            var deletedEntityCount = EntityCountReader.ReadEntityCount(reader);

            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = entityGhostContainer.FetchEntity(entityId);

                deletedEntities.Add(deletedEntity);
            }

#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaCreatedSync)
            {
                throw new("out of sync");
            }
#endif

            var createdEntities = new List<EntityId>();
            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);

            EntityCountWriter.WriteEntityCount(createdEntityCount, undoWriter);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);

                EntityIdWriter.Write(undoWriter, entityId); // Set as deleted entity for undo

                var entityArchetype = new ArchetypeId(reader.ReadUInt16());
                var entityToDeserialize = entityGhostContainer.CreateGhostEntity(entityArchetype, entityId);

                createdEntities.Add(entityToDeserialize);
                // TODO: entityToDeserialize.CompleteEntity.DeserializeAll(reader);
            }
#if DEBUG
            undoWriter.WriteUInt8(Constants.SnapshotDeltaCreatedSync);
#endif
            EntityCountWriter.WriteEntityCount(deletedEntityCount, undoWriter);
            foreach (var deletedEntity in deletedEntities)
            {
                EntityIdWriter.Write(undoWriter, deletedEntity.Id);
                // TODO: deletedEntity.CompleteEntity.SerializeAll(undoWriter);

                entityGhostContainer.DeleteEntity(deletedEntity);
            }

#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaUpdatedSync)
            {
                throw new("out of sync");
            }
#endif


            var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = EntityCountReader.ReadEntityCount(reader);
#if DEBUG
            undoWriter.WriteUInt8(Constants.SnapshotDeltaUpdatedSync);
#endif
            EntityCountWriter.WriteEntityCount(updatedEntityCount, undoWriter);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityToDeserialize = entityGhostContainer.FetchEntity(entityId);


                EntityIdWriter.Write(undoWriter, entityId);
                // TODO: var entityMaskChanged = entityToDeserialize.CompleteEntity.Deserialize(reader);
                // TODO: entityToDeserialize.CompleteEntity.SerializePrevious(entityMaskChanged, undoWriter);
                // TODO: var updatedEntity = new SnapshotDeltaReaderInfoEntity(entityToDeserialize, entityMaskChanged);
                //updatedEntities.Add(updatedEntity);
            }

            EntityCountWriter.WriteEntityCount(0, undoWriter);

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
    */
}