/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Generator
{
    public static class GenerateSerializers
    {
        public static string PrimitiveDeSerializer(string primitiveTypeName)
        {
            return $"reader.Read{primitiveTypeName}()";
        }

        public static string PrimitiveSerializer(string primitiveTypeName, string variableName)
        {
            return $"writer.Write{primitiveTypeName}({variableName})";
        }


        public static string DeSerializeMethodForValueTypes(Type type)
        {
            return $"{type.Name}Reader.Read(reader)";
        }

        public static string SerializeMethodForValueTypes(Type type, string variableName)
        {
            return $"{type.Name}Writer.Write({variableName}, writer)";
        }


        public static string DeSerializeMethod(Type type)
        {
            if (type == typeof(bool))
            {
                return "reader.ReadUInt8() != 0";
            }

            if (type == typeof(byte))
            {
                return PrimitiveDeSerializer("UInt8");
            }


            if (type == typeof(ushort))
            {
                return PrimitiveDeSerializer("UInt16");
            }

            if (type == typeof(uint))
            {
                return PrimitiveDeSerializer("UInt32");
            }

            if (type == typeof(ulong))
            {
                return PrimitiveDeSerializer("UInt64");
            }

            return DeSerializeMethodForValueTypes(type);
        }


        public static string SerializeMethod(Type type, string variableName)
        {
            if (type == typeof(bool))
            {
                return $"writer.WriteUInt8({variableName} ? (byte)1 : (byte)0)";
            }

            if (type == typeof(byte))
            {
                return PrimitiveSerializer("UInt8", variableName);
            }

            if (type == typeof(ushort))
            {
                return PrimitiveSerializer("UInt16", variableName);
            }

            if (type == typeof(uint))
            {
                return PrimitiveSerializer("UInt32", variableName);
            }

            if (type == typeof(ulong))
            {
                return PrimitiveSerializer("UInt64", variableName);
            }

            return SerializeMethodForValueTypes(type, variableName);
        }

        public static string PrimitiveBitSerializer(uint bitCount, string variableName)
        {
            return $"writer.WriteBits({variableName}, {bitCount})";
        }

        public static string PrimitiveBitDeSerializer(Type type, bool includeCast)
        {
            int bitCount;

            if (type == typeof(byte) || type == typeof(sbyte))
            {
                bitCount = 8;
            }
            else if (type == typeof(ushort) || type == typeof(short))
            {
                bitCount = 16;
            }
            else if (type == typeof(uint) || type == typeof(int))
            {
                bitCount = 32;
            }
            else if (type == typeof(ulong) || type == typeof(long))
            {
                bitCount = 64;
            }
            else
            {
                throw new($"unknown type {type.Name}");
            }

            var castString = includeCast ? $"({type.Name})" : "";

            return $"{castString}reader.ReadBits({bitCount})";
        }

        public static string BitSerializeMethod(Type type, string variableName)
        {
            if (type == typeof(bool))
            {
                return $"writer.WriteBits({variableName} ? 1U : 0U, 1)";
            }

            if (type == typeof(byte))
            {
                return PrimitiveBitSerializer(8, variableName);
            }

            if (type == typeof(ushort))
            {
                return PrimitiveBitSerializer(16, variableName);
            }

            if (type == typeof(uint))
            {
                return PrimitiveBitSerializer(32, variableName);
            }

            if (type == typeof(ulong))
            {
                return PrimitiveBitSerializer(64, variableName);
            }

            return SerializeMethodForValueTypes(type, variableName);
        }

        public static string BitDeSerializeMethod(Type type, bool includeCast = true)
        {
            if (type == typeof(bool))
            {
                return "reader.ReadBits(1) != 0";
            }

            if (type == typeof(byte))
            {
                return PrimitiveBitDeSerializer(type, includeCast);
            }

            if (type == typeof(ushort))
            {
                return PrimitiveBitDeSerializer(type, includeCast);
            }

            if (type == typeof(uint))
            {
                return PrimitiveBitDeSerializer(type, includeCast);
            }

            if (type == typeof(ulong))
            {
                return PrimitiveBitDeSerializer(type, includeCast);
            }

            return DeSerializeMethodForValueTypes(type);
        }
    }
}