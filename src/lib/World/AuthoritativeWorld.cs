/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Piot.Surge.Entities;

namespace Piot.Surge
{
    public class AuthoritativeWorld : IEntityContainerWithDetectChanges, IAuthoritativeEntityContainer
    {
        public readonly List<IEntity> allEntities = new();
        protected readonly List<IEntity> created = new();
        protected readonly List<IEntity> deleted = new();

        ushort lastEntityId;
        public Dictionary<ulong, IEntity> Entities { get; } = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntity SpawnEntity(ICompleteEntity completeEntity)
        {
            var freeEntityId = FindFreeEntityId();

            var createdEntity = AddEntity(new(freeEntityId), completeEntity);

            return createdEntity;
        }

        public IEntity[] Created => created.ToArray();
        public IEntity[] Deleted => deleted.ToArray();

        IEntity[] IEntityContainer.AllEntities => allEntities.ToArray();
        public uint EntityCount => (uint)allEntities.Count;

        public T? FindEntity<T>(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var entity);
            if (entity == null)
            {
                return default;
            }

            return (T)entity.CompleteEntity;
        }

        public IEntity? FindEntity(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var entity);
            return entity;
        }

        public T FetchEntity<T>(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var entity);
            if (entity == null)
            {
                throw new NullReferenceException($"could not find entity {entityId}");
            }

            return (T)entity.CompleteEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEntity IEntityContainer.FetchEntity(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var completeNetworkEntity);
            if (completeNetworkEntity == null)
            {
                throw new($"could not find entity {entityId}");
            }

            return completeNetworkEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IEntityContainer.DeleteEntity(EntityId entityId)
        {
            var existingEntity = Entities[entityId.Value];
            if (existingEntity == null)
            {
                throw new($"unknown entity id {entityId}");
            }

            DeleteEntity(existingEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteEntity(IEntity existingEntity)
        {
            existingEntity.Mode = EntityMode.Deleted;
            deleted.Add(existingEntity);
            Entities.Remove(existingEntity.Id.Value);
            allEntities.Remove(existingEntity);
        }

        public void ClearDelta()
        {
            foreach (var entity in created)
            {
                entity.Mode = EntityMode.Normal;
            }

            created.Clear();
            deleted.Clear();
        }

        public virtual IEntity AddEntity(EntityId id, ICompleteEntity completeEntity)
        {
            var entity = new Entity(id, completeEntity);
            created.Add(entity);
            Entities.Add(id.Value, entity);
            allEntities.Add(entity);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort FindFreeEntityId()
        {
            for (var i = 0; i < 200; ++i)
            {
                lastEntityId += 151;
                if (lastEntityId == 0)
                {
                    continue;
                }

                if (!Entities.ContainsKey(lastEntityId))
                {
                    return lastEntityId;
                }
            }

            throw new("Could not find free entity id");
        }
    }
}