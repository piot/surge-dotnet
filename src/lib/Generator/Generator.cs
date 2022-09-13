/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Reflection;
using System.Text;

namespace Piot.Surge.Generator
{
    public static class Generator
    {
        public static string Suffix(string a, string b)
        {
            return a + b;
        }

        public static string FullName(Type t)
        {
            return t.FullName!.Replace('+', '.');
        }

        public static string ShortName(Type t)
        {
            return t.Name;
        }

        public static string FullName(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType?.FullName + "." + methodInfo.Name;
        }

        public static string TitleCase(string input)
        {
            return string.Concat(input[0].ToString().ToUpper(), input.Substring(1, input.Length - 1));
        }

        public static string Indent(int indent)
        {
            return new(' ', indent * 4);
        }


        public static void AddRegionBegin(StringBuilder sb, string name, int indent = 0)
        {
            sb.Append(@$"
#region {name}
{Indent(indent)}// --------------- {name} ---------------").Append(@"
");
        }

        public static void AddRegionEnd(StringBuilder sb, int indent = 0)
        {
            sb.Append($@"
{Indent(indent)}//--------------------------------------
#endregion

");
        }


        public static void AddClassDeclaration(StringBuilder sb, string className, int indent = 0)
        {
            sb.Append($"{Indent(indent)}public sealed class {className}").Append(@$"
{Indent(indent)}{{
");
        }

        public static void AddStaticClassDeclaration(StringBuilder sb, string className, int indent = 0)
        {
            sb.Append($"{Indent(indent)}public static class {className}").Append($@"
{Indent(indent)}{{
");
        }

        public static void AddClassDeclaration(StringBuilder sb, string className, string inheritFrom, int indent = 0)
        {
            sb.Append($"{Indent(indent)}public sealed class {className} : {inheritFrom}").Append($@"
{Indent(indent)}{{
");
        }

        public static void AddStructDeclaration(StringBuilder sb, string structName, string inheritFrom, int indent = 0)
        {
            sb.Append($"{Indent(indent)}public struct {structName} : {inheritFrom}").Append($@"
{Indent(indent)}{{
");
        }

        public static void AddEndDeclaration(StringBuilder sb, int indent = 0)
        {
            sb.Append(@$"
{Indent(indent)}}}
");
        }
    }
}