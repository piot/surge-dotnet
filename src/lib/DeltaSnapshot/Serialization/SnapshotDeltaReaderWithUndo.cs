/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Entities;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public class SnapshotDeltaReaderInfoEntity
    {
        public readonly ulong changeMask;
        public readonly IEntity entity;

        public SnapshotDeltaReaderInfoEntity(IEntity entity, ulong changeMask)
        {
            this.entity = entity;
            this.changeMask = changeMask;
        }
    }

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
        public static (IEntity[], IEntity[], SnapshotDeltaReaderInfoEntity[]) ReadWithUndo(IOctetReader reader,
            IEntityContainerWithGhostCreator entityGhostContainer,
            IOctetWriter undoWriter)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaSync)
            {
                throw new Exception("out of sync");
            }

            undoWriter.WriteUInt8(Constants.SnapshotDeltaSync);
#endif
            var deletedEntities = new List<IEntity>();
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
                throw new Exception("out of sync");
            }
#endif

            var createdEntities = new List<IEntity>();
            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);

            EntityCountWriter.WriteEntityCount(createdEntityCount, undoWriter);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);

                EntityIdWriter.Write(undoWriter, entityId); // Set as deleted entity for undo

                var entityArchetype = new ArchetypeId(reader.ReadUInt16());
                var entityToDeserialize = entityGhostContainer.CreateGhostEntity(entityArchetype, entityId);

                createdEntities.Add(entityToDeserialize);
                entityToDeserialize.DeserializeAll(reader);
            }
#if DEBUG
            undoWriter.WriteUInt8(Constants.SnapshotDeltaCreatedSync);
#endif
            EntityCountWriter.WriteEntityCount(deletedEntityCount, undoWriter);
            foreach (var deletedEntity in deletedEntities)
            {
                EntityIdWriter.Write(undoWriter, deletedEntity.Id);
                deletedEntity.SerializeAll(undoWriter);

                entityGhostContainer.DeleteEntity(deletedEntity);
            }

#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaUpdatedSync)
            {
                throw new Exception("out of sync");
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
                var entityMaskChanged = entityToDeserialize.Deserialize(reader);
                entityToDeserialize.SerializePrevious(entityMaskChanged, undoWriter);
                var updatedEntity = new SnapshotDeltaReaderInfoEntity(entityToDeserialize, entityMaskChanged);
                updatedEntities.Add(updatedEntity);
            }

            EntityCountWriter.WriteEntityCount(0, undoWriter);

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}