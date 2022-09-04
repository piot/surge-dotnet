/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Entities;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateBitReader
    {
        public static IEntity[] ReadAndApply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.CompleteSnapshotStartMarker);
#endif
            if (entityGhostContainerWithCreator.AllEntities.Length != 0)
            {
                throw new Exception("can not read complete state unless world is completely empty");
            }

            var createdEntities = new List<IEntity>();
            var createdEntityCount = EntityCountReader.ReadEntityCount(reader);
            for (var i = 0; i < createdEntityCount; ++i)
            {
                var entityId = EntityIdReader.Read(reader);
                var entityArchetype = new ArchetypeId((ushort)reader.ReadBits(10));

                var entityToDeserialize =
                    entityGhostContainerWithCreator.CreateGhostEntity(entityArchetype, entityId);
                entityToDeserialize.DeserializeAll(reader);
                entityToDeserialize.FireCreated();
                createdEntities.Add(entityToDeserialize);
            }

            return createdEntities.ToArray();
        }
    }
}