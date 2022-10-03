/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateEntityBitReader
    {
        public static void Apply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator, bool notifyWorldSync, ILog log)
        {
            if (entityGhostContainerWithCreator.EntityCount != 0)
            {
                log.Notice(
                    "can not read complete state unless world is completely empty. If you see this for a longer period of time, something is wrong");
                throw new DeserializeException(
                    "can not read complete state unless world is completely empty. If you see this for a longer period of time, something is wrong");
            }

            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId((ushort)reader.ReadBits(10));

                var entityToDeserialize =
                    entityGhostContainerWithCreator.CreateGhostEntity(entityArchetype, entityId);
                entityToDeserialize.CompleteEntity.DeserializeAll(reader);
                //createdEntities.Add(entityToDeserialize);
                if (notifyWorldSync)
                {
                    entityGhostContainerWithCreator.AddGhostEntity(entityToDeserialize);
                }
            }
        }
    }
}