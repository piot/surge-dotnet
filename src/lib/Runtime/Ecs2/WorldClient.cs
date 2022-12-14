/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.Surge.Core;

namespace Piot.Surge.Ecs2
{
    public struct ChangeInformation
    {
        public uint[] createdEntityIds;
        public uint[] destroyedEntityIds;
        public uint[] modifiedEntityIds;
    }
    public class EcsWorldClient : IEcsWorldFetcher, IDataReceiver
    {
        readonly Dictionary<uint, ClientEntityInfo> entities = new();
        readonly ILog log;

        readonly List<uint> createdEntities = new();
        readonly List<uint> deletedEntities = new();
        readonly HashSet<uint> modifiedEntities = new();

        public EcsWorldClient(ILog log)
        {
            this.log = log;
            Components = Array.Empty<object>();
            entities = new();
        }
        public void Update<T>(uint mask, uint entityId, T data) where T : struct
        {
            log.Debug($"Received an update : entity {entityId} data: {data} with mask {mask}");
            modifiedEntities.Add(entityId);

            entities[entityId].Set(data);
        }

        public T Grab<T>(uint entityId) where T : struct
        {
            var foundExistingEntity = entities.TryGetValue(entityId, out var entityInfo);
            if (!foundExistingEntity || entityInfo is null)
            {
                throw new($"could not find existing {entityId}");
            }

            return entityInfo.Grab<T>();
        }
        public T GrabOrCreate<T>(uint entityId) where T : struct
        {
            var foundExistingEntity = entities.TryGetValue(entityId, out var entityInfo);
            if (!foundExistingEntity || entityInfo is null)
            {
                entityInfo = new ClientEntityInfo();
                entities[entityId] = entityInfo;
            }
            return entityInfo.GrabOrCreate<T>();
        }

        public void ReceiveNew<T>(uint entityId, T data) where T : struct
        {
            log.Debug($"Received Full entityId: {entityId} data:{data}");

            var foundExistingEntity = entities.TryGetValue(entityId, out var existingEntityInfo);
            if (!foundExistingEntity || existingEntityInfo is null)
            {
                existingEntityInfo = new();
                entities[entityId] = existingEntityInfo;
                createdEntities.Add(entityId);
            }
            modifiedEntities.Add(entityId);
            existingEntityInfo.Set(data);
        }

        public void DestroyComponent<T>(uint entityId) where T : struct
        {
            log.Debug($"Received destroy for entity {entityId} and type {typeof(T).Name}");
            var entityInfo = entities[entityId];
            entityInfo.DestroyComponent<T>();
            if (entityInfo.components.Count == 0)
            {
                deletedEntities.Add(entityId);
                entities.Remove(entityId);
            }
            else
            {
                modifiedEntities.Add(entityId);
            }
        }

        public void Reset()
        {
            entities.Clear();
        }
        
        public ChangeInformation Changes()
        {
            var change = new ChangeInformation
            {
                createdEntityIds = createdEntities.ToArray(), destroyedEntityIds = deletedEntities.ToArray(), modifiedEntityIds = modifiedEntities.ToArray()
            };
            createdEntities.Clear();
            deletedEntities.Clear();
            modifiedEntities.Clear();
            return change;
        }
        
        public IEnumerable<object> Components { get; }

        T? IEcsWorldFetcher.Get<T>(uint entityId)
        {
            var foundExistingEntity = entities.TryGetValue(entityId, out var entityInfo);
            if (!foundExistingEntity || entityInfo is null)
            {
                return null;
            }

            return entityInfo.Get<T>();
        }

        public bool HasComponent<T>(uint entityId) where T : struct
        {
            var foundExistingEntity = entities.TryGetValue(entityId, out var existingEntityInfo);
            if (!foundExistingEntity || existingEntityInfo is null)
            {
                return false;
            }
            
            return existingEntityInfo.HasComponent<T>();
        }
    }
}