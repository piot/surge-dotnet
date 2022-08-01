/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.ChangeMaskSerialization;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public class SnapshotDeltaReader
    {
        public static uint ReadEntityCount(IOctetReader reader)
        {
            return reader.ReadUInt16();
        }

        public static (IEntity[], IEntity[], IEntity[]) Read(IOctetReader reader, IEntityContainer creation)
        {
            var deletedEntities = new List<IEntity>();
            var deletedEntityCount = ReadEntityCount(reader);
            for (var i = 0; i < deletedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var deletedEntity = creation.FetchEntity(entityId);
                deletedEntities.Add(deletedEntity);
                creation.DeleteEntity(entityId);
            }

            var createdEntities = new List<IEntity>();
            var createdEntityCount = ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId(reader.ReadUInt16());
                var entityToDeserialize = creation.CreateEntity(entityArchetype, entityId);
                entityToDeserialize.DeserializeAll(reader);
            }

            var updatedEntities = new List<IEntity>();
            var updatedEntityCount = ReadEntityCount(reader);
            for (var i = 0; i < updatedEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var serializeMask = FullChangeMaskReader.ReadFullChangeMask(reader);

                var entityToDeserialize = creation.FetchEntity(entityId);
                entityToDeserialize.Deserialize(serializeMask.mask, reader);
                updatedEntities.Add(entityToDeserialize);
            }

            return (deletedEntities.ToArray(), createdEntities.ToArray(), updatedEntities.ToArray());
        }
    }
}