/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.FieldMask;
using Surge.Types;

namespace Piot.Surge.Ecs2
{
    public class HostEntityInfo
    {
        public readonly Dictionary<uint, ComponentInfo> components = new();
        public readonly HashSet<uint> destroyedComponents = new();

        public String64 Name { get; set; } = new("");
        

        public uint[] DestroyedComponents()
        {
            var array = destroyedComponents.ToArray();
            destroyedComponents.Clear();
            return array;
        }
        
        public void Set<T>(T data) where T : struct
        {

            var foundComponent = components.TryGetValue(DataIdLookup<T>.value, out var componentInfo);
            if (!foundComponent || componentInfo is null)
            {
                if (DataDiffer<T>.diff is null)
                {
                    throw new($"I have no information about {typeof(T).Name}");
                }

                var allFieldsChangedMask = DataDiffer<T>.diff(default, data);
                componentInfo = new()
                {
                    componentWriter = new ComponentWriter<T>
                    {
                        data = data
                    },
                    changedFieldMask = allFieldsChangedMask,
                    Data = data
                };
                components.Add(DataIdLookup<T>.value, componentInfo);
                return;
            }

            if (componentInfo.changedFieldMask == ChangedFieldsMask.DeletedMaskBit)
            {
                // It was previously deleted, but lets wake it up again
                componentInfo.changedFieldMask = 0;
                componentInfo.Data = new T();
                if (componentInfo.Data is null)
                {
                    throw new Exception($"internal error. we just set the data, but it is still null, {componentInfo.changedFieldMask}");
                }
            }

            if (componentInfo.Data is null)
            {
                throw new Exception($"internal error. data is null, but it has been set before {componentInfo.changedFieldMask}");
            }

            componentInfo.componentWriter!.Data = data;

            if (componentInfo.Data is null)
            {
                //Debug.LogError($"ComponentInfo.Data is null {componentInfo.changedFieldMask}");
            }
            
            var fieldChangeMask = DataDiffer<T>.diff!.Invoke(data, (T)componentInfo.Data!);

            componentInfo.changedFieldMask |= fieldChangeMask;
            componentInfo.Data = data;
        }

        public bool HasComponent<T>() where T : struct
        {
            return components.ContainsKey(DataIdLookup<T>.value);
        }

        public bool HasComponent(ushort id)
        {
            return components.ContainsKey(id);
        }

        public void WriteMask(IBitWriter writer, uint componentTypeId, ulong mask)
        {
            if (!components.ContainsKey(componentTypeId))
            {
                throw new Exception($"{componentTypeId} {mask}");
            }
            components[componentTypeId].componentWriter!.WriteMask(writer, mask);
        }

        public ComponentInfo? GetComponent(uint componentTypeId)
        {
            var foundComponent = components.TryGetValue(componentTypeId, out var componentInfo);
            if (!foundComponent || componentInfo is null)
            {
                return null;
            }

            return componentInfo;
        }
        
        public T? Get<T>() where T : struct
        {
            var lookup = components.TryGetValue(DataIdLookup<T>.value, out var foundComponent);
            if (!lookup)
            {
                return null;
            }

            if (foundComponent is null)
            {
                throw new($"internal error. component is null {DataIdLookup<T>.value}");
            }

            if (foundComponent.changedFieldMask == ChangedFieldsMask.DeletedMaskBit)
            {
                return null;
            }

            var foundData = (T)foundComponent.Data!;
            return foundData;
        }

        public bool Ref<T>(ref T data) where T : struct
        {
            var lookup = components[DataIdLookup<T>.value];
            if (lookup is null)
            {
                return false;
            }
            if (lookup.changedFieldMask == ChangedFieldsMask.DeletedMaskBit)
            {
                return false;
            }
            
            data = (T)lookup.Data!;

            return true;
        }

        public void Destroy<T>() where T : struct
        {
            var lookup = components.TryGetValue(DataIdLookup<T>.value, out var foundComponent);
            if (!lookup)
            {
                return;
            }

            if (foundComponent is null)
            {
                throw new($"internal error. component is already deleted null {DataIdLookup<T>.value}");
            }
            

            foundComponent.changedFieldMask = ChangedFieldsMask.DeletedMaskBit;
            //foundComponent.Data = null;
            //Debug.Log($"Destroyed component {DataIdLookup<T>.value}, setting data to null and mask to {foundComponent.changedFieldMask}");
        }

        public void DestroyAll()
        {
            foreach (var component in components)
            {
                destroyedComponents.Add(component.Key);
            }
            components.Clear();
        }

        public interface IComponentWriter
        {

            public object Data { get; set; }
            public void WriteFull(IBitWriter writer);
            public void WriteMask(IBitWriter writer, ulong mask);
        }

        public class ComponentInfo
        {
            public ulong changedFieldMask;
            public IComponentWriter? componentWriter;
            public object? Data { get; set; }

            public override string ToString()
            {
                return $"[componentInfo {Data} {changedFieldMask:X8}]";
            }
        }

        class ComponentWriter<T> : IComponentWriter where T : struct
        {
            public T data;
            public uint mask;

            public void WriteFull(IBitWriter writer)
            {
                DataStreamWriter.Write(data, writer);
            }

            public void WriteMask(IBitWriter writer, ulong mask)
            {
                DataStreamWriter.Write(data, writer, (uint)mask);
            }

            public object Data
            {
                set => data = (T)value;
                get => data;
            }
        }
    }

}