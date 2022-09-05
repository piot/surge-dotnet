/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;

namespace Piot.Clog
{
    public class ConsoleOutputLogger : ILogTarget
    {
        private const string resetColor = "\x1b[0m";

        public void Log(LogLevel level, string prefix, string message, object[] args)
        {
            var strings = args.Select(x => x.ToString());
            var values = args.Length > 0 ? $"({string.Join(", ", strings)})" : "";
            var color = ColorStringFromLogLevel(level);

            var line = $"{color}{level,8}{resetColor} : [{prefix}] {message} {values}";

            Console.WriteLine(line);
        }

        private static (bool, Color) ColorValuesFromLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.LowLevel => (false, Color.Default),
                LogLevel.Debug => (false, Color.Blue),
                LogLevel.Info => (false, Color.Cyan),
                LogLevel.Notice => (true, Color.Magenta),
                LogLevel.Warning => (true, Color.Yellow),
                LogLevel.Error => (true, Color.Red),
                _ => throw new Exception("illegal LogLevel")
            };
        }

        private static string ForegroundColorString(bool bright, Color foregroundColor)
        {
            if (foregroundColor == Color.Default)
            {
                return resetColor;
            }

            var prefix = bright ? "1" : "0";
            return "\x1b" + $"[{prefix};{30 + foregroundColor}m";
        }

        public void WriteColorChart()
        {
            string[] colorStrings = { "black", "red", "green", "yellow", "blue", "magenta", "cyan", "white" };

            for (var i = 0; i < 8; ++i)
            {
                var colorName = colorStrings[i];
                Console.WriteLine($"{i} {ForegroundColorString(false, (Color)i)}{colorName}{resetColor}");
                Console.WriteLine($"{i} {ForegroundColorString(true, (Color)i)}bright {colorName}{resetColor}");
            }
        }

        private static string ColorStringFromLogLevel(LogLevel level)
        {
            var (prefix, foregroundColor) = ColorValuesFromLogLevel(level);
            return ForegroundColorString(prefix, foregroundColor);
        }

        private enum Color
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