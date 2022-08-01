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
        private readonly ILogTarget target;
        private readonly LogLevel threshold = LogLevel.Debug;

        public Log(ILogTarget target)
        {
            this.target = target;
        }

        public void Info(string message)
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, Array.Empty<object>());
        }

        public void Info<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, new object[] { arg });
        }

        public void Info<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, new object[] { arg0, arg1 });
        }

        public void Info<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
            where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, new object[]{ arg0, arg1, arg2 });
        }

        public void Info(string message, object[] args)
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, args);
        }

        public void Warn(string message)
        {
            if (threshold > LogLevel.Warning) return;

            target.Log(LogLevel.Warning, message, Array.Empty<object>());
        }

        public void Warn<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Warning) return;

            target.Log(LogLevel.Warning, message, new object[] { arg });
        }

        public void Warn<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Warning) return;

            target.Log(LogLevel.Warning, message, new object[] { arg0, arg1 });
        }

        public void Warn<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2) where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Warning) return;

            target.Log(LogLevel.Warning, message, new object[]{ arg0, arg1, arg2 });
        }

        public void Warn(string message, object[] args)
        {
            if (threshold > LogLevel.Warning) return;

            target.Log(LogLevel.Warning, message, args);
        }

        public void Debug(string message)
        {
            if (threshold > LogLevel.Debug) return;

            target.Log(LogLevel.Debug, message, Array.Empty<object>());
        }

        public void Debug<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Warning) return;

            target.Log(LogLevel.Warning, message, new object[] { arg });
        }

        public void Debug<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Debug) return;

            target.Log(LogLevel.Debug, message, new object[] { arg0, arg1 });
        }

        public void Debug<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2) where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Debug) return;

            target.Log(LogLevel.Debug, message, new object[]{ arg0, arg1, arg2 });
        }

        public void Debug(string message, object[] args)
        {
            if (threshold > LogLevel.Debug) return;

            target.Log(LogLevel.Debug, message, args);
        }

        public void Error(string message)
        {
            if (threshold > LogLevel.Debug) return;

            target.Log(LogLevel.Debug, message, Array.Empty<object>());
        }

        public void Error<T>(string message, T arg) where T : notnull
        {
            if (threshold > LogLevel.Error) return;

            target.Log(LogLevel.Error, message, new object[] { arg });
        }

        public void Error<T0, T1>(string message, T0 arg0, T1 arg1) where T0 : notnull where T1 : notnull
        {
            if (threshold > LogLevel.Error) return;

            target.Log(LogLevel.Error, message, new object[] { arg0, arg1 });
        }

        public void Error<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2) where T0 : notnull where T1 : notnull where T2 : notnull
        {
            if (threshold > LogLevel.Error) return;

            target.Log(LogLevel.Error, message, new object[]{ arg0, arg1, arg2 });
        }

        public void Error(string message, object[] args)
        {
            if (threshold > LogLevel.Error) return;

            target.Log(LogLevel.Error, message, args);
        }
    }
}