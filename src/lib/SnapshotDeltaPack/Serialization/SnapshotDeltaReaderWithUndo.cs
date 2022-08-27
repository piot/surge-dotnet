/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.ChangeMaskSerialization;

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
        private static void WriteEntityCount(uint count, IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)count);
        }

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
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaSync)
            {
                throw new Exception("out of sync");
            }

            undoWriter.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaSync);
#endif
            var deletedEntities = new List<IEntity>();
            var deletedEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);

            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = entityGhostContainer.FetchEntity(entityId);

                deletedEntities.Add(deletedEntity);
            }

#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaCreatedSync)
            {
                throw new Exception("out of sync");
            }
#endif

            var createdEntities = new List<IEntity>();
            var createdEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);

            WriteEntityCount(createdEntityCount, undoWriter);
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
            undoWriter.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaCreatedSync);
#endif
            WriteEntityCount(deletedEntityCount, undoWriter);
            foreach (var deletedEntity in deletedEntities)
            {
                EntityIdWriter.Write(undoWriter, deletedEntity.Id);
                deletedEntity.SerializeAll(undoWriter);

                entityGhostContainer.DeleteEntity(deletedEntity);
            }

#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaUpdatedSync)
            {
                throw new Exception("out of sync");
            }
#endif


            var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);
#if DEBUG
            undoWriter.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaUpdatedSync);
#endif
            WriteEntityCount(updatedEntityCount, undoWriter);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var serializeMask = ChangedFieldsMaskReader.ReadChangedFieldMask(reader);

                var entityToDeserialize = entityGhostContainer.FetchEntity(entityId);


                EntityIdWriter.Write(undoWriter, entityId);
                ChangedFieldsMaskWriter.WriteChangedFieldsMask(undoWriter, serializeMask);
                entityToDeserialize.Serialize(serializeMask.mask, undoWriter);


                entityToDeserialize.Deserialize(serializeMask.mask, reader);
                var updatedEntity = new SnapshotDeltaReaderInfoEntity(entityToDeserialize, serializeMask.mask);
                updatedEntities.Add(updatedEntity);
            }

            WriteEntityCount(0, undoWriter);

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}