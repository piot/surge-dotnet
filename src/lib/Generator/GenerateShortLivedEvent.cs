/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Piot.Surge.Generator
{
    public static class GenerateShortLivedEvent
    {
        public static void AddShortLivedEventValueConstants(StringBuilder sb, IEnumerable<MethodInfo> methodInfos,
            int indent = 0)
        {
            Generator.AddStaticClassDeclaration(sb, "EventArchetypeConstants", indent);

            var i = 1;
            foreach (var methodInfo in methodInfos)
            {
                sb.Append(@$"{Generator.Indent(indent)}public const byte {methodInfo.Name} = {i};
");
                ++i;
            }

            Generator.AddEndDeclaration(sb, indent);
        }
    }
}