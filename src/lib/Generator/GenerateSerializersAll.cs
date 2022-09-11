/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Piot.Surge.Generator
{
    public readonly struct SerializeFieldInfo
    {
        public readonly Type type;
        public readonly string name;

        public SerializeFieldInfo(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }
    }

    /// <summary>
    ///     Generate serialize all for fields in a struct
    /// </summary>
    public static class GenerateSerializersAll
    {
        public static void AddSerializeAll(StringBuilder sb, IEnumerable<SerializeFieldInfo> fieldInfos,
            string methodSuffix, string prefix)
        {
            sb.Append($@"    public void Serialize{methodSuffix}(IOctetWriter writer)
    {{
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.name;
                var completeVariable = $"{prefix}{fieldName}";
                sb.Append(
                    $@"        {GenerateSerializers.SerializeMethod(fieldInfo.type, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddBitSerializeAll(StringBuilder sb, IEnumerable<SerializeFieldInfo> fieldInfos,
            string methodSuffix, string prefix)
        {
            sb.Append($@"    public void Serialize{methodSuffix}(IBitWriter writer)
    {{
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.name;
                var completeVariable = $"{prefix}{fieldName}";
                sb.Append(
                    $@"        {GenerateSerializers.BitSerializeMethod(fieldInfo.type, completeVariable)};
");
            }

            sb.Append(@"    }

");
        }


        public static void AddDeserializeAll(StringBuilder sb, IEnumerable<SerializeFieldInfo> fieldInfos,
            string methodSuffix, string prefix)
        {
            sb.Append($@"    public void DeSerialize{methodSuffix}(IOctetReader reader)
    {{
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.name;
                sb.Append(
                    $@"        {prefix}{fieldName} = {GenerateSerializers.DeSerializeMethod(fieldInfo.type)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddBitDeserializeAll(StringBuilder sb, IEnumerable<SerializeFieldInfo> fieldInfos,
            string methodSuffix, string prefix = "")
        {
            sb.Append($@"    public void DeSerialize{methodSuffix}(IBitReader reader)
    {{
");

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldName = fieldInfo.name;
                sb.Append(
                    $@"        {prefix}{fieldName} = {GenerateSerializers.BitDeSerializeMethod(fieldInfo.type)};
");
            }

            sb.Append(@"    }

");
        }

        public static void AddSerialization(StringBuilder sb, IEnumerable<SerializeFieldInfo> fieldInfos,
            string methodSuffix = "", string prefix = "", int indent = 0)
        {
            AddBitSerializeAll(sb, fieldInfos, methodSuffix, prefix);
            AddSerializeAll(sb, fieldInfos, methodSuffix, prefix);

            AddBitDeserializeAll(sb, fieldInfos, methodSuffix, prefix);
            AddDeserializeAll(sb, fieldInfos, methodSuffix, prefix);
        }
    }
}