/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

// ReSharper disable CheckNamespace

namespace Piot.Clog
{
    public class Log : ILog
    {
        private readonly string prefix;
        private readonly ILogTarget target;
        private readonly LogLevel threshold = LogLevel.Debug;

        public Log(ILogTarget target, string prefix = "")
        {
            this.target = target;
            this.prefix = prefix;
        }

        public void Info(string message)
        {
            if (threshold > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, Array.Empty<object>());
        }

        public void Info<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[] { arg });
        }

        public void Info<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[] { arg0, arg1 });
        }

        public void Info<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Info(string message, object[] args)
        {
            if (threshold > LogLevel.Info)
            {
                return;
            }

            target.Log(LogLevel.Info, prefix, message, args);
        }

        public void Warn(string message)
        {
            if (threshold > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, Array.Empty<object>());
        }

        public void Warn<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[] { arg });
        }

        public void Warn<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[] { arg0, arg1 });
        }

        public void Warn<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Warn(string message, object[] args)
        {
            if (threshold > LogLevel.Warning)
            {
                return;
            }

            target.Log(LogLevel.Warning, prefix, message, args);
        }

        public void Debug(string message)
        {
            if (threshold > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, Array.Empty<object>());
        }

        public void Debug<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[] { arg });
        }

        public void Debug<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[] { arg0, arg1 });
        }

        public void Debug<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Debug(string message, object[] args)
        {
            if (threshold > LogLevel.Debug)
            {
                return;
            }

            target.Log(LogLevel.Debug, prefix, message, args);
        }

        public void DebugLowLevel(string message)
        {
            if (threshold > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, Array.Empty<object>());
        }

        public void DebugLowLevel<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[] { arg });
        }

        public void DebugLowLevel<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[] { arg0, arg1 });
        }

        public void DebugLowLevel<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void DebugLowLevel(string message, object[] args)
        {
            if (threshold > LogLevel.LowLevel)
            {
                return;
            }

            target.Log(LogLevel.LowLevel, prefix, message, args);
        }


        public void Notice(string message)
        {
            if (threshold > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, Array.Empty<object>());
        }

        public void Notice<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[] { arg });
        }

        public void Notice<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[] { arg0, arg1 });
        }

        public void Notice<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Notice(string message, object[] args)
        {
            if (threshold > LogLevel.Notice)
            {
                return;
            }

            target.Log(LogLevel.Notice, prefix, message, args);
        }


        public void Error(string message)
        {
            if (threshold > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, Array.Empty<object>());
        }

        public void Error<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[] { arg });
        }

        public void Error<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[] { arg0, arg1 });
        }

        public void Error<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, new object[] { arg0, arg1, arg2 });
        }

        public void Error(string message, object[] args)
        {
            if (threshold > LogLevel.Error)
            {
                return;
            }

            target.Log(LogLevel.Error, prefix, message, args);
        }

        public ILog SubLog(string debugPrefix)
        {
            return new Log(target, prefix != "" ? "/" + debugPrefix : debugPrefix);
        }
    }
}