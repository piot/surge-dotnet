/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Surge.Core;

namespace Ecs2
{
    public class EcsWorldClient : IEcsWorldFetcher, IDataReceiver
    {
        readonly Dictionary<uint, ClientEntityInfo> entities = new();
        readonly ILog log;

        public EcsWorldClient(ILog log)
        {
            this.log = log;
            Components = Array.Empty<object>();
            entities = new();
        }
        public void Update<T>(uint mask, uint entityId, T data) where T : struct
        {
            log.Debug($"Received an update : entity {entityId} data: {data} with mask {mask}");
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



        public void ReceiveNew<T>(uint entityId, T data) where T : struct
        {
            log.Debug($"Received Full entityId: {entityId} data:{data}");

            var foundExistingEntity = entities.TryGetValue(entityId, out var existingEntityInfo);
            if (!foundExistingEntity || existingEntityInfo is null)
            {
                existingEntityInfo = new();
                entities[entityId] = existingEntityInfo;
            }

            existingEntityInfo.Set(data);
        }

        public void DestroyComponent<T>(uint entityId) where T : struct
        {
            log.Debug($"Received destroy for entity {entityId} and type {typeof(T).Name}");
            var entityInfo = entities[entityId];
            entityInfo.DestroyComponent<T>();
            if (entityInfo.components.Count == 0)
            {
                entities.Remove(entityId);
            }
        }

        public void Reset()
        {
            entities.Clear();
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
            throw new NotImplementedException();
        }
    }
}