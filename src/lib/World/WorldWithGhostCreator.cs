/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;

namespace Piot.Surge
{
    public class WorldWithGhostCreator : AuthoritativeWorld, IEntityContainerWithGhostCreator
    {
        private readonly IEntityGhostCreator creator;
        private readonly INotifyWorld notifyWorld;

        private ulong lastEntityId;

        public WorldWithGhostCreator(IEntityGhostCreator creator, INotifyWorld notifyWorld)
        {
            this.creator = creator;
            this.notifyWorld = notifyWorld;
        }

        public bool IsAuthoritative => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEntity IEntityGhostCreator.CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
        {
            var newEntity = creator.CreateGhostEntity(archetypeId, entityId);

            created.Add(newEntity);
            Entities.Add(entityId.Value, newEntity);
            notifyWorld.NotifyCreation(newEntity.GeneratedEntity);

            return newEntity;
        }
    }
}