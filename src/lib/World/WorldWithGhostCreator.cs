/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;
using Piot.Surge.Entities;

namespace Piot.Surge
{
    public sealed class WorldWithGhostCreator : AuthoritativeWorld, IEntityContainerWithGhostCreator
    {
        readonly IEntityGhostCreator creator;
        readonly INotifyContainerReset notifyContainerReset;
        readonly INotifyEntityCreation notifyEntityCreation;

        public WorldWithGhostCreator(IEntityGhostCreator creator, INotifyEntityCreation notifyEntityCreation,
            INotifyContainerReset notifyContainerReset,
            bool isAuthoritative)
        {
            this.notifyContainerReset = notifyContainerReset;
            IsAuthoritative = isAuthoritative;
            this.creator = creator;
            this.notifyEntityCreation = notifyEntityCreation;
        }

        public bool IsAuthoritative { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEntity IEntityGhostCreator.CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
        {
            var newEntity = creator.CreateGhostEntity(archetypeId, entityId);

            created.Add(newEntity);
            Entities.Add(entityId.Value, newEntity);
            allEntities.Add(newEntity);

            return newEntity;
        }

        public void AddGhostEntity(IEntity entity)
        {
            notifyEntityCreation.NotifyCreation(entity.CompleteEntity);
        }

        public void Reset()
        {
            Entities.Clear();
            allEntities.Clear();
            created.Clear();
            deleted.Clear();

            notifyContainerReset.NotifyReset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IEntity AddEntity(EntityId id, ICompleteEntity completeEntity)
        {
            return base.AddEntity(id, completeEntity);
        }
    }
}