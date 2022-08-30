/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Piot.Surge.Entities;
using Piot.Surge.GeneratedEntity;

namespace Piot.Surge
{
    public class AuthoritativeWorld : IEntityContainerWithDetectChanges, IAuthoritativeEntityContainer
    {
        protected readonly List<IEntity> created = new();
        private readonly List<IEntity> deleted = new();

        private ushort lastEntityId;

        public Dictionary<ulong, IEntity> Entities { get; } = new();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntity SpawnEntity(IGeneratedEntity generatedEntity)
        {
            var freeEntityId = FindFreeEntityId();

            return AddEntity(new EntityId(freeEntityId), generatedEntity);
        }

        public IEntity[] Created => created.ToArray();
        public IEntity[] Deleted => deleted.ToArray();

        IEntity[] IEntityContainer.AllEntities => Entities.Values.ToArray();

        public T? FindEntity<T>(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var entity);
            if (entity == null)
            {
                return default;
            }

            return (T)entity.GeneratedEntity;
        }

        public T FetchEntity<T>(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var entity);
            if (entity == null)
            {
                throw new NullReferenceException($"could not find entity {entityId}");
            }

            return (T)entity.GeneratedEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEntity IEntityContainer.FetchEntity(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var completeNetworkEntity);
            if (completeNetworkEntity == null)
            {
                throw new Exception($"could not find entity {entityId}");
            }

            return completeNetworkEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IEntityContainer.DeleteEntity(EntityId entityId)
        {
            var existingEntity = Entities[entityId.Value];
            if (existingEntity == null)
            {
                throw new Exception($"unknown entity id {entityId}");
            }

            DeleteEntity(existingEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteEntity(IEntity existingEntity)
        {
            existingEntity.Mode = EntityMode.Deleted;
            deleted.Add(existingEntity);
            Entities.Remove(existingEntity.Id.Value);
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

        internal IEntity? FindEntity(EntityId entityId)
        {
            Entities.TryGetValue(entityId.Value, out var entity);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntity AddEntity(EntityId id, IGeneratedEntity generatedEntity)
        {
            var entity = new Entity(id, generatedEntity);
            created.Add(entity);
            Entities.Add(id.Value, entity);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort FindFreeEntityId()
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

            throw new Exception("Could not find free entity id");
        }
    }
}