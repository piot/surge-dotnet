/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Entities;
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaReader
    {
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
            if (reader.ReadUInt8() != Constants.SnapshotDeltaSync)
            {
                throw new("out of sync");
            }
#endif
            var deletedEntities = new List<IEntity>();
            var deletedEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = entityGhostContainerWithCreator.FetchEntity(entityId);
                deletedEntities.Add(deletedEntity);
                entityGhostContainerWithCreator.DeleteEntity(entityId);
            }


#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaCreatedSync)
            {
                throw new("out of sync");
            }
#endif
            var createdEntities = new List<IEntity>();
            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId(reader.ReadUInt16());
                var entityToDeserialize = entityGhostContainerWithCreator.CreateGhostEntity(entityArchetype, entityId);
                entityToDeserialize.CompleteEntity.DeserializeAll(reader);
                createdEntities.Add(entityToDeserialize);
            }


#if DEBUG
            if (reader.ReadUInt8() != Constants.SnapshotDeltaUpdatedSync)
            {
                throw new("out of sync");
            }
#endif

            var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityToDeserialize = entityGhostContainerWithCreator.FetchEntity(entityId);
                var serializeMask = entityToDeserialize.CompleteEntity.Deserialize(reader);
                updatedEntities.Add(new(entityToDeserialize, serializeMask));
            }

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}