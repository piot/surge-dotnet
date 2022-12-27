/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.Core;
using Piot.Surge.DeltaSnapshot.ComponentFieldMask;
using Surge.Game;
using Surge.Types;

namespace Piot.Surge.Ecs2
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

    public class WorldHost : IEcsWorldFetcher, IDataSender, IDataReceiver, IEntityContainerToWorld, IEntityContainerWithDetectChanges
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


        void IDataReceiver.Reset()
        {
            throw new NotImplementedException();
        }
        void IDataReceiver.ReceiveNew<T>(uint entityId, T data)
        {
            var foundExistingEntity = entities.TryGetValue(entityId, out var existingEntityInfo);
            if (!foundExistingEntity || existingEntityInfo is null)
            {
                existingEntityInfo = new();
                entities[entityId] = existingEntityInfo;
            }

            modifiedEntities.Add(entityId);
            existingEntityInfo.Set(data);
        }
        void IDataReceiver.Update<T>(uint mask, uint entityId, T data)
        {
            entities[entityId].Set(data);
        }
        T IDataReceiver.GrabOrCreate<T>(uint entityId)
        {
            throw new NotImplementedException();
        }

        public void DestroyComponent<T>(uint entityId) where T : struct
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);

            hostEntityInfo.Destroy<T>();
            modifiedEntities.Add(entityId);
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
            var hostEntityInfo = FindHostEntityInfo(entityId);
            if (hostEntityInfo is null)
            {
                return false;
            }

            return hostEntityInfo.HasComponent(componentTypeId);
        }

        public void WriteMask(IBitWriter writer, uint entityId, ushort componentTypeId, ulong mask)
        {
            var componentInfo = GetComponentInfo(entityId, componentTypeId);
            componentInfo.componentWriter!.WriteMask(writer, mask);
        }

        public void WriteFull(IBitWriter writer, uint entityId, ushort componentTypeId)
        {
            var componentInfo = GetComponentInfo(entityId, componentTypeId);
            componentInfo.componentWriter!.WriteFull(writer);
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
                throw new("entity id was not there");
            }

            var component = hostEntityInfo.Get<T>();
            if (component is null)
            {
                throw new("component was not there");
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

        public void SetName(uint entityId, String64 name)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);
            hostEntityInfo.Name = name;
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
                throw new($"could not find entity {entityId}");
            }

            return hostEntityInfo;
        }

        HostEntityInfo.ComponentInfo GetComponentInfo(uint entityId, ushort componentTypeId)
        {
            var hostEntity = FindHostEntityInfo(entityId);
            if (hostEntity is null)
            {
                throw new($"could not find component {componentTypeId} on entity {entityId}. Entity does not exist");
            }

            var foundComponent = hostEntity.GetComponent(componentTypeId);
            if (foundComponent is null)
            {
                throw new($"could not find component {componentTypeId}  on entity {entityId}");
            }

            return foundComponent;
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

            log.Debug($"entity {entityId} {hostEntityInfo.Name} : component {DataIdLookup<T>.value} ({typeof(T).FullName}) is modified!");
            modifiedEntities.Add(entityId);

            hostEntityInfo.Set(data);
        }

        public uint[] DestroyedComponents(uint entityId)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);
            return hostEntityInfo.DestroyedComponents();
        }

        public void DestroyEntity(uint entityId)
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);

            hostEntityInfo.DestroyAll();
        }


        public void WriteFull(IBitWriter writer, uint entityId)
        {
            /*
            foreach (var component in entities[entityId].components)
            {
                entities[entityId].WriteMask(writer, component.Key, uint.MaxValue);
            }
            */
        }

        public void Ref<T>(uint entityId, ref T data) where T : struct
        {
            var hostEntityInfo = GetHostEntityInfo(entityId);
            hostEntityInfo.Ref(ref data);
        }
    }
}