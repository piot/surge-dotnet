/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Piot.Clog;

namespace Piot.Surge.Generator
{
    public static class Scanner
    {
        public static bool HasAttribute<T>(Type t) where T : Attribute
        {
            return t.GetCustomAttribute<T>() != null;
        }

        public static bool IsStruct(Type t)
        {
            return t.IsValueType && !t.IsEnum;
        }

        public static IEnumerable<Type> ScanForLogics(ILog output)
        {
            var assemblies2 = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> allTypes = new();
            foreach (var assembly in assemblies2) allTypes.AddRange(assembly.GetTypes());

            var logicClasses = allTypes
                .Where(type => IsStruct(type) && HasAttribute<LogicAttribute>(type)).ToArray();

            return logicClasses;
        }
    }
}