/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

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

        public void Info<T>(string message, T arg)
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, new object[] { arg });
        }

        public void Info<T0, T1>(string message, T0 arg0, T1 arg1)
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, new object[] { arg0, arg1 });
        }

        public void Info<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, new object[] { arg0, arg1, arg2 });
        }

        public void Info<T0, T1, T2>(string message, object[] args)
        {
            if (threshold > LogLevel.Info) return;

            target.Log(LogLevel.Info, message, args);
        }
    }
}