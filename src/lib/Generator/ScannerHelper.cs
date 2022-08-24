/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Reflection;

namespace Piot.Surge.Generator
{
    public static class ScannerHelper
    {
        public static bool HasAttribute<T>(Type t) where T : Attribute
        {
            return t.GetCustomAttribute<T>() != null;
        }

        public static bool IsStruct(Type t)
        {
            return t.IsValueType && !t.IsEnum;
        }

        public static MethodInfo ImplementedMethod(Type t, string methodName)
        {
            var methodInfo = t.GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new Exception($"method {methodName} was not found in {t.Name}");
            }

            if (methodInfo.IsAbstract)
            {
                throw new Exception($"method {methodName} can not be abstract in {t.Name}");
            }

            return methodInfo;
        }
    }
}