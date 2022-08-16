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
        public ulong changeMask;
        public IEntity entity;

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
        ///     Similar to SnapshotDeltaReader.Read, but it also writes the current values of the fields that are overwritten.
        ///     The undoWriter can be used to undo the overwritten entities.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="creation"></param>
        /// <param name="undoWriter"></param>
        /// <returns></returns>
        public static (IEntity[], IEntity[], SnapshotDeltaReaderInfoEntity[]) ReadWithUndo(IOctetReader reader,
            IEntityContainer creation,
            IOctetWriter undoWriter)
        {
#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaSync)
            {
                throw new Exception("out of sync");
            }
#endif
            undoWriter.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaSync);
            var deletedEntities = new List<IEntity>();
            var deletedEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);

            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = creation.FetchEntity(entityId);

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
                var entityToDeserialize = creation.CreateEntity(entityArchetype, entityId);

                createdEntities.Add(entityToDeserialize);
                entityToDeserialize.DeserializeAll(reader);
            }

            undoWriter.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaCreatedSync);

            WriteEntityCount(deletedEntityCount, undoWriter);
            foreach (var deletedEntity in deletedEntities)
            {
                EntityIdWriter.Write(undoWriter, deletedEntity.Id);
                deletedEntity.SerializeAll(undoWriter);

                creation.DeleteEntity(deletedEntity);
            }

#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaUpdatedSync)
            {
                throw new Exception("out of sync");
            }
#endif


            var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);
            undoWriter.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaUpdatedSync);
            WriteEntityCount(updatedEntityCount, undoWriter);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var serializeMask = ChangedFieldsMaskReader.ReadFullChangeMask(reader);

                var entityToDeserialize = creation.FetchEntity(entityId);


                EntityIdWriter.Write(undoWriter, entityId);
                ChangedFieldsMaskWriter.WriteChangedFieldsMask(undoWriter, serializeMask);
                entityToDeserialize.Serialize(serializeMask.mask, undoWriter);


                entityToDeserialize.Deserialize(serializeMask.mask, reader);
                var updatedEntity = new SnapshotDeltaReaderInfoEntity(entityToDeserialize, serializeMask.mask);
                updatedEntities.Add(updatedEntity);
            }

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}