/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.DeltaSnapshot.ComponentFieldMask;
using Surge.Game;

namespace Ecs2
{
    public struct SerializeComponentInfo
    {
        public ushort componentTypeId;
        public ulong mask;
    }

    public interface IDataSender
    {
        public void WriteMask(IBitWriter writer, uint entityId, SerializeComponentInfo[] masks);
        public bool HasComponentTypeId(uint entityId, ushort componentTypeId);

        public uint[] AllEntities();
        public void WriteMask(IBitWriter writer, uint entityId, ushort componentTypeId, ulong masks);
        public void WriteFull(IBitWriter writer, uint entityId, ushort componentTypeId);
    }

    public class WorldHost : IEcsWorldFetcher, IDataSender, IEntityContainerToWorld, IEntityContainerWithDetectChanges
    {
        readonly Dictionary<uint, HostEntityInfo> entities = new();

        readonly ILog log;

        readonly HashSet<uint> modifiedEntities = new();
        ushort entityId;

        public WorldHost(ILog log)
        {
            this.log = log;
            entities = new();
            modifiedEntities = new();
            Components = Array.Empty<object>();
        }


        public uint[] AllEntities()
        {
            return entities.Keys.ToArray();
        }


        public void WriteMask(IBitWriter writer, uint entityId, SerializeComponentInfo[] masks)
        {
            foreach (var maskInfo in masks)
            {
                entities[entityId].WriteMask(writer, maskInfo.componentTypeId, maskInfo.mask);
            }
        }
        public bool HasComponentTypeId(uint entityId, ushort componentTypeId)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);

            return hostEntityInfo.HasComponent(componentTypeId);
        }

        public void WriteMask(IBitWriter writer, uint entityId, ushort componentTypeId, ulong mask)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);
            hostEntityInfo.WriteMask(writer, componentTypeId, mask);
        }

        public void WriteFull(IBitWriter writer, uint entityId, ushort componentTypeId)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);
            hostEntityInfo.WriteFull(writer, componentTypeId);
        }

        public IEnumerable<object> Components { get; }

        public bool HasComponent<T>(uint entityId) where T : struct
        {
            var existing = GetHostEntityInfo(entityId);

            return existing.HasComponent<T>();
        }

        public T? Get<T>(uint entityId) where T : struct
        {
            var hostEntityInfo = FindHostEntityInfo(entityId);
            return hostEntityInfo?.Get<T>();
        }
        
        
        public T Grab<T>(uint entityId) where T : struct
        {
            var hostEntityInfo = FindHostEntityInfo(entityId);
            if (hostEntityInfo is null)
            {
                throw new Exception("entity id was not there");
            }
            var component = hostEntityInfo.Get<T>();
            if (component is null)
            {
                throw new Exception("component was not there");
            }

            return component.Value;
        }
        
        ushort[] IEcsContainer.AllEntities => entities.Keys.Select(x => (ushort)x).ToArray();


        SerializableComponent[] IEcsContainer.Components(uint entityId)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);

            return hostEntityInfo.components.Select(x => new SerializableComponent(new((ushort)x.Key), x.Value.componentWriter!)).ToArray();
        }

        public AllEntitiesChangesThisTick EntitiesThatHasChanged(ILog log)
        {
            var allChanges = new AllEntitiesChangesThisTick();

            foreach (var changedEntityId in modifiedEntities)
            {
                var entityInfo = entities[changedEntityId];
                var entityTarget = new EntityChangesForOneEntity(new((ushort)changedEntityId));
                allChanges.EntitiesComponentChanges.Add(changedEntityId, entityTarget);
                foreach (var componentInfoPair in entityInfo.components)
                {
                    if (componentInfoPair.Value.changedFieldMask != 0)
                    {
                        entityTarget.Add(new((ushort)componentInfoPair.Key), new(componentInfoPair.Value.changedFieldMask));
                        componentInfoPair.Value.changedFieldMask = 0;
                    }
                }
            }

            // TODO: Assemble everything
            ClearChanges();

            return allChanges;
        }

        public EntityId CreateEntity()
        {
            entityId++;

            entities.Add(entityId, new());

            return new(entityId);
        }

        void ClearChanges()
        {
            modifiedEntities.Clear();
        }

        HostEntityInfo GetHostEntityInfo(uint entityId)
        {
            var hostEntityInfo = FindHostEntityInfo((ushort)entityId);
            if (hostEntityInfo is null)
            {
                throw new("this");
            }

            return hostEntityInfo;
        }

        HostEntityInfo? FindHostEntityInfo(uint entityId)
        {
            var found = entities.TryGetValue((ushort)entityId, out var hostEntityInfo);
            if (!found)
            {
                return null;
            }

            if (hostEntityInfo is null)
            {
                throw new("null host entity info added to dictionary");
            }

            return hostEntityInfo;
        }

        public void Set<T>(uint entityId, T data) where T : struct
        {
            var hostEntityInfo = FindHostEntityInfo(entityId);
            if (hostEntityInfo is null)
            {
                hostEntityInfo = new();
                entities[entityId] = hostEntityInfo;
            }

            log.Debug($"entity {entityId} is modified!");
            modifiedEntities.Add(entityId);

            hostEntityInfo.Set(data);
        }

        public void DestroyComponent<T>(uint entityId) where T : struct
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);

            hostEntityInfo.Destroy<T>();
            modifiedEntities.Add(entityId);
        }

        public void DestroyEntity(uint entityId)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);

            hostEntityInfo.DestroyAll();
        }

        public void WriteFull(IBitWriter writer, uint entityId)
        {
            foreach (var component in entities[entityId].components)
            {
                entities[entityId].WriteMask(writer, component.Key, uint.MaxValue);
            }
        }

        public void Ref<T>(uint entityId, ref T data) where T : struct
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);
            hostEntityInfo.Ref(ref data);
        }
    }
}