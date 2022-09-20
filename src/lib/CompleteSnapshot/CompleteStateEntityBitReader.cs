/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateEntityBitReader
    {
        public static void Apply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator)
        {
            if (entityGhostContainerWithCreator.EntityCount != 0)
            {
                throw new("can not read complete state unless world is completely empty");
            }

            // var createdEntities = new List<IEntity>();
            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId((ushort)reader.ReadBits(10));

                var entityToDeserialize =
                    entityGhostContainerWithCreator.CreateGhostEntity(entityArchetype, entityId);
                entityToDeserialize.CompleteEntity.DeserializeAll(reader);
                //createdEntities.Add(entityToDeserialize);
                entityGhostContainerWithCreator.AddGhostEntity(entityToDeserialize);
            }

            //return createdEntities.ToArray();
        }
    }
}