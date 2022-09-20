/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Piot.Surge.Generator
{
    public readonly struct ShortLivedEventInterface
    {
        public readonly Type eventInterface;
        public readonly IEnumerable<MethodInfo> methodInfos;

        public ShortLivedEventInterface(Type eventInterface, IEnumerable<MethodInfo> methodInfos)
        {
            this.eventInterface = eventInterface;
            this.methodInfos = methodInfos;
        }
    }

    public static class ShortLivedEventsCollector
    {
        public static ShortLivedEventInterface Collect(Type shortLivedEventsInterface)
        {
            var methodsInInterface = shortLivedEventsInterface.GetMethods();
            List<MethodInfo> methods = new();
            foreach (var method in methodsInInterface)
            {
                if (method.ReturnType != typeof(void))
                {
                    throw new("Short lived events interface is only allowed to have void as return");
                }

                if (method.IsStatic)
                {
                    throw new("static makes no sense for the interface");
                }

                methods.Add(method);
            }

            return new(shortLivedEventsInterface, methods);
        }
    }
}