/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

// ReSharper disable CheckNamespace

namespace Piot.Clog
{
    public class Log : ILog, ILogConfiguration
    {
        private readonly string prefix;
        private readonly ILogTarget target;

        public Log(ILogTarget target, LogLevel logLevel = LogLevel.Debug, string prefix = "")
        {
            LogLevel = logLevel;
            this.target = target;
            this.prefix = prefix;
        }

        public void Info(string message)
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, Array.Empty<object>());
        }

        public void Info<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[] { arg });
        }

        public void Info<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[] { arg0, arg1 });
        }

        public void Info<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Info(string message, object[] args)
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, args);
        }

        public void Warn(string message)
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, Array.Empty<object>());
        }

        public void Warn<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[] { arg });
        }

        public void Warn<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[] { arg0, arg1 });
        }

        public void Warn<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Warn(string message, object[] args)
        {
            if (LogLevel > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, args);
        }

        public void Debug(string message)
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, Array.Empty<object>());
        }

        public void Debug<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[] { arg });
        }

        public void Debug<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[] { arg0, arg1 });
        }

        public void Debug<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Debug(string message, object[] args)
        {
            if (LogLevel > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, args);
        }

        public void DebugLowLevel(string message)
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, Array.Empty<object>());
        }

        public void DebugLowLevel<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[] { arg });
        }

        public void DebugLowLevel<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[] { arg0, arg1 });
        }

        public void DebugLowLevel<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void DebugLowLevel(string message, object[] args)
        {
            if (LogLevel > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, args);
        }


        public void Notice(string message)
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, Array.Empty<object>());
        }

        public void Notice<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[] { arg });
        }

        public void Notice<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[] { arg0, arg1 });
        }

        public void Notice<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Notice(string message, object[] args)
        {
            if (LogLevel > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, args);
        }


        public void Error(string message)
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, Array.Empty<object>());
        }

        public void Error<T>(string message, T arg) where T : notnull
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[] { arg });
        }

        public void Error<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[] { arg0, arg1 });
        }

        public void Error<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Error(string message, object[] args)
        {
            if (LogLevel > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, args);
        }

        public ILog SubLog(string debugPrefix)
        {
            return new Log(target, LogLevel, prefix != "" ? "/" + debugPrefix : debugPrefix);
        }

        public LogLevel LogLevel { get; set; } = LogLevel.LowLevel;
    }
}