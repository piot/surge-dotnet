/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;

namespace Piot.Surge
{
    public class ClientWorld : AuthoritativeWorld, IEntityCreation
    {
        private readonly IEntityCreation creator;

        private ulong lastEntityId;

        public ClientWorld(IEntityCreation creator)
        {
            this.creator = creator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEntity IEntityCreation.CreateEntity(ArchetypeId archetypeId, EntityId entityId)
        {
            var newEntity = creator.CreateEntity(archetypeId, entityId);

            created.Add(newEntity);
            Entities.Add(entityId.Value, newEntity);

            return newEntity;
        }
    }
}