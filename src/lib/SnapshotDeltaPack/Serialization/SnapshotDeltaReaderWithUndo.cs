/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.ChangeMaskSerialization;
using Piot.Surge.OctetSerialize;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnnapshotDeltaPack.Serialization
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

        public static (IEntity[], IEntity[], SnapshotDeltaReaderInfoEntity[]) ReadWithUndo(IOctetReader reader,
            IEntityContainer creation,
            IOctetWriter undoWriter)
        {
            var deletedEntities = new List<IEntity>();
            var deletedEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);

            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = creation.FetchEntity(entityId);

                deletedEntities.Add(deletedEntity);
            }

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

            WriteEntityCount(deletedEntityCount, undoWriter);
            foreach (var deletedEntity in deletedEntities)
            {
                EntityIdWriter.Write(undoWriter, deletedEntity.Id);
                deletedEntity.SerializeAll(undoWriter);

                creation.DeleteEntity(deletedEntity);
            }

            var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = SnapshotDeltaReader.ReadEntityCount(reader);
            WriteEntityCount(updatedEntityCount, undoWriter);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var serializeMask = FullChangeMaskReader.ReadFullChangeMask(reader);

                var entityToDeserialize = creation.FetchEntity(entityId);


                EntityIdWriter.Write(undoWriter, entityId);
                FullChangeMaskWriter.WriteFullChangeMask(undoWriter, serializeMask);
                entityToDeserialize.Serialize(serializeMask.mask, undoWriter);


                entityToDeserialize.Deserialize(serializeMask.mask, reader);
                var updatedEntity = new SnapshotDeltaReaderInfoEntity(entityToDeserialize, serializeMask.mask);
                updatedEntities.Add(updatedEntity);
            }

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}