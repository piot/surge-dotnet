/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Piot.Clog
{
    public static class ArgumentReplace
    {
        public static ICollection<Match> FindMatches(string input)
        {
            return Regex.Matches(input, @"\{[a-zA-Z0-9\:]+\}");
        }

        public static string ArgumentSectionsToString(ArgumentSection[] argumentSections)
        {
            var index = 0;
            var s = "";

            foreach (var argument in argumentSections)
            {
                if (index > 0)
                {
                    s += ", ";
                }

                s += $"{argument.fieldName}={argument.value}";
                index++;
            }

            return s;
        }

        public static (string, string) ReplaceArguments(string inputString, object[] arguments)
        {
            var (removedString, argumentSections) = ScanArguments(inputString, arguments, true);
            var argumentString = ArgumentSectionsToString(argumentSections);

            return (removedString, argumentString);
        }

        public static string ReplaceArgumentsWithValues(string inputString, object[] arguments)
        {
            var (removedString, _) = ScanArguments(inputString, arguments, true);

            return removedString;
        }

        public static (string, ArgumentSection[]) ScanArguments(string inputString, object[] arguments,
            bool insertValue)
        {
            var matches = FindMatches(inputString);
            var removedInput = inputString;

            var index = 0;

            var argumentSections = new List<ArgumentSection>();
            var indexOffset = 0;
            foreach (var match in matches)
            {
                var fieldName = inputString.Substring(match.Index + 1, match.Length - 2);
                removedInput = removedInput.Remove(match.Index - indexOffset, match.Length);
                var parts = fieldName.Split(":");
                fieldName = parts[0];

                var formatting = parts.Length > 1 ? parts[1] : "";

                var valueObject = arguments[index];
                var value = valueObject.ToString()!;

                if (formatting.Length > 0)
                {
                    var completeFormat = $"{{0:{formatting}}}";
                    value = string.Format(completeFormat, valueObject);
                }

                if (insertValue)
                {
                    var insertString = $"<i>{fieldName}</i>=<b><color=lightblue>{value}</color></b>";

                    removedInput = removedInput.Insert(match.Index - indexOffset, insertString);
                    indexOffset -= insertString.Length;
                }

                indexOffset += match.Length;

                argumentSections.Add(new(fieldName, value, match.Index, match.Length));
                index++;
            }

            return (removedInput, argumentSections.ToArray());
        }

        public readonly struct ArgumentSection
        {
            public readonly string fieldName;
            public readonly string value;
            public readonly int startIndex;
            public readonly int length;

            public ArgumentSection(string fieldName, string value, int startIndex, int length)
            {
                this.fieldName = fieldName;
                this.value = value;
                this.startIndex = startIndex;
                this.length = length;
            }
        }
    }
}