/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Piot.Clog
{
    public sealed class ConsoleOutputLogger : ILogTarget
    {
        const string ResetColor = "\x1b[0m";


        public void Log(LogLevel level, string prefix, string message, object[] args)
        {
            var strings = args.Select(Utils.ArgumentValueToString);
            var values = args.Length > 0 ? $"({string.Join(", ", strings)})" : "";
            var color = ColorStringFromLogLevel(level);

            var line = $"{color}{level,8}{ResetColor} : [{prefix}] {message} {values}";

            Console.WriteLine(line);
        }

        static (bool, Color) ColorValuesFromLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.LowLevel => (false, Color.Default),
                LogLevel.Debug => (false, Color.Blue),
                LogLevel.Info => (false, Color.Cyan),
                LogLevel.Notice => (false, Color.Magenta),
                LogLevel.Warning => (true, Color.Yellow),
                LogLevel.Error => (true, Color.Red),
                _ => throw new("illegal LogLevel")
            };
        }

        static string ForegroundColorString(bool bright, Color foregroundColor)
        {
            if (foregroundColor == Color.Default)
            {
                return ResetColor;
            }

            var prefix = bright ? "1" : "0";
            return "\x1b" + $"[{prefix};{30 + foregroundColor}m";
        }

        public void WriteColorChart()
        {
            string[] colorStrings =
            {
                "black", "red", "green", "yellow", "blue", "magenta", "cyan", "white"
            };

            for (var i = 0; i < 8; ++i)
            {
                var colorName = colorStrings[i];
                Console.WriteLine($"{i} {ForegroundColorString(false, (Color)i)}{colorName}{ResetColor}");
                Console.WriteLine($"{i} {ForegroundColorString(true, (Color)i)}bright {colorName}{ResetColor}");
            }
        }

        static string ColorStringFromLogLevel(LogLevel level)
        {
            var (prefix, foregroundColor) = ColorValuesFromLogLevel(level);
            return ForegroundColorString(prefix, foregroundColor);
        }

        enum Color
        {
            Default = -1,
            Black = 0,
            Red = 1,
            Green = 2,
            Yellow = 3,
            Blue = 4,
            Magenta = 5,
            Cyan = 6,
            White = 7
        }
    }
}