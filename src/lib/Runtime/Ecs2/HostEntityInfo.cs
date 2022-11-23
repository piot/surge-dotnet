/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.Core;

namespace Ecs2
{
    public class HostEntityInfo
    {
        public readonly Dictionary<uint, ComponentInfo> components = new();

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

            componentInfo.componentWriter!.Data = data;

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
            components[componentTypeId].componentWriter!.WriteMask(writer, mask);
        }

        public void WriteFull(IBitWriter writer, uint componentTypeId)
        {
            components[componentTypeId].componentWriter!.WriteFull(writer);
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

            data = (T)lookup.Data!;

            return true;
        }

        public void Destroy<T>() where T : struct
        {
            components.Remove(DataIdLookup<T>.value);
        }

        public void DestroyAll()
        {
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