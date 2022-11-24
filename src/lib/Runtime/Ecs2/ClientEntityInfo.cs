/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System.Collections.Generic;
using Piot.Surge.Core;

namespace Ecs2
{
    public class ClientEntityInfo
    {

        public readonly Dictionary<ushort, ComponentInfo> components = new();

        public T Grab<T>() where T : struct
        {
            return (T)components[DataIdLookup<T>.value].data!;
        }

        public T? Get<T>() where T : struct
        {
            var found = components.TryGetValue(DataIdLookup<T>.value, out var foundComponentInfo);
            if (!found)
            {
                return null;
            }

            if (foundComponentInfo is null)
            {
                return null;
            }

            return (T)foundComponentInfo.data!;
        }

        public bool Ref<T>(ref T data) where T : struct
        {
            var found = components[DataIdLookup<T>.value];
            if (found is null)
            {
                return false;
            }

            data = (T)found.data!;
            return true;
        }

        public void Set<T>(T data) where T : struct
        {
            var dataId = DataIdLookup<T>.value;
            var found = components.TryGetValue(dataId, out var existingComponentInfo);
            if (found)
            {
                components.Remove(dataId);
            }

            var componentInfo = new ComponentInfo
            {
                data = data
            };

            components.Add(dataId, componentInfo);
        }

        public void DestroyComponent<T>() where T : struct
        {
            components.Remove(DataIdLookup<T>.value);
        }

        public bool HasComponent<T>() where T : struct
        {
            var found = components.TryGetValue(DataIdLookup<T>.value, out _);
            return found;
        }

        public class ComponentInfo
        {
            public object? data;
        }
    }
}