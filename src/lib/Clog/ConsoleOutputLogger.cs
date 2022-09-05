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

        private static (bool, int) ColorValuesFromLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.LowLevel => (false, 4),
                LogLevel.Debug => (true, 6),
                LogLevel.Info => (true, 2),
                LogLevel.Notice => (true, 5),
                LogLevel.Warning => (true, 3),
                LogLevel.Error => (false, 1),
                _ => throw new Exception("illegal LogLevel")
            };
        }

        private static string ForegroundColorString(bool bright, int foregroundColor)
        {
            var prefix = bright ? "1" : "0";
            return "\x1b" + $"[{prefix};{30 + foregroundColor}m";
        }

        public void WriteColorChart()
        {
            string[] colorStrings = { "black", "red", "green", "yellow", "blue", "magenta", "cyan", "white" };

            for (var i = 0; i < 8; ++i)
            {
                var colorName = colorStrings[i];
                Console.WriteLine($"{i} {ForegroundColorString(false, i)}{colorName}{resetColor}");
                Console.WriteLine($"{i} {ForegroundColorString(true, i)}bright {colorName}{resetColor}");
            }
        }

        private static string ColorStringFromLogLevel(LogLevel level)
        {
            var (prefix, foregroundColor) = ColorValuesFromLogLevel(level);
            return ForegroundColorString(prefix, foregroundColor);
        }
    }
}