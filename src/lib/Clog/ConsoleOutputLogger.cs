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
        public void Log(LogLevel level, string prefix, string message, object[] args)
        {
            var strings = args.Select(x => x.ToString());
            var values = args.Length > 0 ? $"({string.Join(", ", strings)})" : "";
            var color = ColorStringFromLogLevel(level);
            const string resetColor = "\x1b[0m";
            var line = $"{color}{level,8}{resetColor} : [{prefix}] {message} {values}";

            Console.WriteLine(line);
        }

        private static (int, int) ColorValuesFromLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.LowLevel => (0, 34),
                LogLevel.Debug => (1, 36),
                LogLevel.Info => (1, 32),
                LogLevel.Notice => (1, 35),
                LogLevel.Warning => (1, 33),
                LogLevel.Error => (0, 31),
                _ => throw new Exception("illegal LogLevel")
            };
        }

        private static string ColorStringFromLogLevel(LogLevel level)
        {
            var (prefix, foregroundColor) = ColorValuesFromLogLevel(level);
            return "\x1b" + $"[{prefix};{foregroundColor}m";
        }
    }
}