/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;
using Piot.Surge.Entities;

namespace Piot.Surge
{
    public class WorldWithGhostCreator : AuthoritativeWorld, IEntityContainerWithGhostCreator
    {
        private readonly IEntityGhostCreator creator;
        private readonly INotifyWorld notifyWorld;

        private ulong lastEntityId;

        public WorldWithGhostCreator(IEntityGhostCreator creator, INotifyWorld notifyWorld, bool isAuthoritative)
        {
            IsAuthoritative = isAuthoritative;
            this.creator = creator;
            this.notifyWorld = notifyWorld;
        }

        public bool IsAuthoritative { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEntity IEntityGhostCreator.CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
        {
            var newEntity = creator.CreateGhostEntity(archetypeId, entityId);

            created.Add(newEntity);
            Entities.Add(entityId.Value, newEntity);
            allEntities.Add(newEntity);
            notifyWorld.NotifyCreation(newEntity.GeneratedEntity);

            return newEntity;
        }
    }
}