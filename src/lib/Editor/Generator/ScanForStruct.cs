/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CecilEx;
using Piot.Clog;

namespace Piot.Surge.Generator
{
    public static class AttributeScanner
    {
        /// <summary>
        ///     Scans .NET type information to find types that have a specific Attribute attached.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static IEnumerable<TypeDefinition> ScanForStructWithAttribute(ILog log, IEnumerable<AssemblyDefinition> assemblies, Type attributeToScanFor)
        {
            List<TypeDefinition> foundLogicStructs = new();
            foreach (var assembly in assemblies)
            {
                var logicClasses = assembly.MainModule.Types
                    .Where(type => ScannerHelper.IsStruct(type) && ScannerHelper.HasAttribute(type, attributeToScanFor.Name))
                    .ToArray();

                log.Debug($"In assembly '{assembly.MainModule.Name}' logics detected: {logicClasses.Length}");

                foundLogicStructs.AddRange(logicClasses);
            }

            return foundLogicStructs;
        }
    }
}