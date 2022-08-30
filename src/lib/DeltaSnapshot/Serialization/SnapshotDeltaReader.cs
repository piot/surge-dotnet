/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.FieldMask.Serialization;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaReader
    {
        public static uint ReadEntityCount(IOctetReader reader)
        {
            var count = reader.ReadUInt16();
            if (count > 128)
            {
                throw new Exception($"suspicious count {count}");
            }

            return count;
        }

        /// <summary>
        ///     Reading a snapshot delta pack and returning the created, deleted and updated entities.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entityContainer"></param>
        /// <returns></returns>
        public static (IEntity[], IEntity[], SnapshotDeltaReaderInfoEntity[]) Read(IOctetReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator)
        {
#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaSync)
            {
                throw new Exception("out of sync");
            }
#endif
            var deletedEntities = new List<IEntity>();
            var deletedEntityCount = ReadEntityCount(reader);
            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = entityGhostContainerWithCreator.FetchEntity(entityId);
                deletedEntities.Add(deletedEntity);
                entityGhostContainerWithCreator.DeleteEntity(entityId);
            }


#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaCreatedSync)
            {
                throw new Exception("out of sync");
            }
#endif
            var createdEntities = new List<IEntity>();
            var createdEntityCount = ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId(reader.ReadUInt16());
                var entityToDeserialize = entityGhostContainerWithCreator.CreateGhostEntity(entityArchetype, entityId);
                entityToDeserialize.DeserializeAll(reader);
                createdEntities.Add(entityToDeserialize);
            }


#if DEBUG
            if (reader.ReadUInt8() != SnapshotSerialization.Constants.SnapshotDeltaUpdatedSync)
            {
                throw new Exception("out of sync");
            }
#endif

            var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = ReadEntityCount(reader);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var serializeMask = ChangedFieldsMaskReader.ReadChangedFieldMask(reader);

                var entityToDeserialize = entityGhostContainerWithCreator.FetchEntity(entityId);
                entityToDeserialize.Deserialize(serializeMask.mask, reader);
                updatedEntities.Add(new SnapshotDeltaReaderInfoEntity(entityToDeserialize, serializeMask.mask));
            }

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}