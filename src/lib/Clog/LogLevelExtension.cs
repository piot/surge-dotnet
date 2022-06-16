/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Clog
{
    public static class LogLevelExtension
    {
        public static string ToString(this LogLevel level)
        {
            return level switch
            {
                LogLevel.LowLevel => "LowLevel",
                LogLevel.Debug => "Debug",
                LogLevel.Info => "Info",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }
    }
}