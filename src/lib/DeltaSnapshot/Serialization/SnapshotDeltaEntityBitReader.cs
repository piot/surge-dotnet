/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaEntityBitReader
    {
        public static void ReadAndApply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator, bool isOverlappingMergedSnapshot)
        {
#if DEBUG
            if (reader.ReadBits(8) != Constants.SnapshotDeltaSync)
            {
                throw new Exception("out of sync");
            }
#endif
            //var deletedEntities = new List<IEntity>();
            var deletedEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                if (isOverlappingMergedSnapshot)
                {
                    var alreadyDeletedEntity = entityGhostContainerWithCreator.FindEntity(entityId);
                    if (alreadyDeletedEntity is null || !alreadyDeletedEntity.IsAlive)
                    {
                        // It has already been deleted
                        continue;
                    }
                }

                var deletedEntity = entityGhostContainerWithCreator.FetchEntity(entityId);
                //deletedEntities.Add(deletedEntity);
                entityGhostContainerWithCreator.DeleteEntity(entityId);
            }


#if DEBUG
            if (reader.ReadBits(8) != Constants.SnapshotDeltaCreatedSync)
            {
                throw new Exception("out of sync");
            }
#endif
            //var createdEntities = new List<IEntity>();
            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId((ushort)reader.ReadBits(10));
                if (isOverlappingMergedSnapshot)
                {
                    var alreadyExistingEntity = entityGhostContainerWithCreator.FindEntity(entityId);
                    if (alreadyExistingEntity is not null)
                    {
                        alreadyExistingEntity.CompleteEntity.DeserializeAll(reader);
                        continue;
                    }
                }

                var entityToDeserialize =
                    entityGhostContainerWithCreator.CreateGhostEntity(entityArchetype, entityId);
                entityToDeserialize.CompleteEntity.DeserializeAll(reader);
                //createdEntities.Add(entityToDeserialize);
                entityGhostContainerWithCreator.AddGhostEntity(entityToDeserialize);
            }


#if DEBUG
            if (reader.ReadBits(8) != Constants.SnapshotDeltaUpdatedSync)
            {
                throw new Exception("out of sync");
            }
#endif

            //var updatedEntities = new List<SnapshotDeltaReaderInfoEntity>();
            var updatedEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityToDeserialize = entityGhostContainerWithCreator.FetchEntity(entityId);
                var serializeMask = entityToDeserialize.CompleteEntity.Deserialize(reader);
//                updatedEntities.Add(new SnapshotDeltaReaderInfoEntity(entityToDeserialize, serializeMask));
                entityToDeserialize.CompleteEntity.FireChanges(serializeMask);
            }
        }
    }
}