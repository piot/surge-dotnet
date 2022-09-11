/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;

namespace Piot.Surge.Generator
{
    public static class ShortLivedEventsScanner
    {
        /// <summary>
        ///     Scans .NET type information to find types that have the LogicAttribute attached.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static IEnumerable<Type> ScanForEventInterfaces(ILog output)
        {
            var assemblies2 = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> allTypes = new();
            foreach (var assembly in assemblies2)
            {
                allTypes.AddRange(assembly.GetTypes());
            }

            var eventInterfaces = allTypes
                .Where(type =>
                    ScannerHelper.IsInterface(type) && ScannerHelper.HasAttribute<ShortLivedEventsAttribute>(type))
                .ToArray();

            return eventInterfaces;
        }
    }
}