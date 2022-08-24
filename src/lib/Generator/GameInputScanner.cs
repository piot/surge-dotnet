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
    public static class GameInputScanner
    {
        /// <summary>
        ///     Scans .NET type information to find types that have the GameInputAttribute attached.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static IEnumerable<Type> ScanForGameInputs(ILog output)
        {
            var assemblies2 = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> allTypes = new();
            foreach (var assembly in assemblies2)
            {
                allTypes.AddRange(assembly.GetTypes());
            }

            var gameInputStructs = allTypes
                .Where(type => ScannerHelper.IsStruct(type) && ScannerHelper.HasAttribute<InputAttribute>(type))
                .ToArray();

            return gameInputStructs;
        }
    }
}